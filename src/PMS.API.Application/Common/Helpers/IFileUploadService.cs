
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace PMS.API.Application.Common.Helpers;

public interface IFileUploadService
{
  Task<string> UploadFileAsync(byte[] fileData, string fileName);
  Task<(bool result, string ImageUrl)> UploadImage(IFormFile file, string? prefix = "");
  Task<(bool result, string ImageUrl)> UploadImageFromBase64(string base64Image, string? prefix = "");
  Task<bool> DeleteImage(string fileKey);
  Task<List<string>> UploadImage(List<IFormFile> files, string? prefix = "");
}
