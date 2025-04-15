namespace PMS.API.SharedKernel.Extensions;
public static class FileExtensions
{
  public static string GetUniqueFileName(this string fileName)
  {
    return Guid.NewGuid().ToString() + fileName;
  }
}
