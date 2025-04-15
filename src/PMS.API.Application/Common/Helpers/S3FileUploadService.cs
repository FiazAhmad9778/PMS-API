using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;

namespace PMS.API.Application.Common.Helpers;

public class S3FileUploadService
{
  private readonly IAmazonS3 _s3Client;
  private readonly IConfiguration _configuration;

  public S3FileUploadService(IAmazonS3 s3Client, IConfiguration configuration)
  {
    _s3Client = s3Client;
    _configuration = configuration;
  }

  public async Task<string> UploadFileAsync(byte[] fileData, string fileName)
  {
    var s3Key = "uploads/" + fileName;

    using (var memoryStream = new MemoryStream(fileData))
    {
      string bucketName = _configuration.GetValue<string>("AWS:BucketName")!;
      if (string.IsNullOrEmpty(bucketName))
      {
        throw new Exception("Bucket Name not found!");
      }

      // Check bucket Exists
      if (!(await IsBucketExist(bucketName)))
      {
        // If not create a new bucket.
        await CreateBucket(bucketName);
      }

      var request = new PutObjectRequest
      {
        BucketName = bucketName,
        Key = s3Key,
        InputStream = memoryStream
      };

      var response = await _s3Client.PutObjectAsync(request);

      if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
      {
        return s3Key;
      }

      throw new Exception($"Failed to upload file to Amazon S3. Error: {response.HttpStatusCode}");
    }
  }

  public async Task<bool> IsBucketExist(string bucketName)
  {
    try
    {
      var response = await _s3Client.ListBucketsAsync();
      return response.Buckets.Exists(b => string.Equals(b.BucketName, bucketName, StringComparison.OrdinalIgnoreCase));
    }
    catch (AmazonS3Exception ex)
    {
      Console.WriteLine($"Error checking bucket existence: {ex.Message}");
      return false;
    }
  }

  public async Task CreateBucket(string bucketName)
  {
    try
    {
      var request = new PutBucketRequest
      {
        BucketName = bucketName,
        UseClientRegion = true
      };

      var response = await _s3Client.PutBucketAsync(request);

      if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
      {
        throw new Exception($"Failed to create bucket. Error: {response.HttpStatusCode}");
      }
    }
    catch (AmazonS3Exception ex)
    {
      throw new Exception($"Error creating bucket: {ex.Message}");
    }
  }

  public async Task<bool> DeleteFileAsync(string fileKey)
  {
    try
    {
      string bucketName = _configuration.GetValue<string>("AWS:BucketName")!;
      if (string.IsNullOrEmpty(bucketName))
      {
        throw new Exception("Bucket Name not found!");
      }

      var request = new DeleteObjectRequest
      {
        BucketName = bucketName,
        Key = fileKey
      };

      var response = await _s3Client.DeleteObjectAsync(request);

      return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
    }
    catch (AmazonS3Exception ex)
    {
      Console.WriteLine($"Error deleting file: {ex.Message}");
      return false;
    }
  }
}
