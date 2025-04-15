using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PMS.API.SharedKernel.Helpers;

public static class RandomGeneratorHelper
{
  public static string GenerateSecureRandomString(int length)
  {
    const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    using (var rng = RandomNumberGenerator.Create())
    {
      byte[] randomBytes = new byte[length];
      rng.GetBytes(randomBytes);

      StringBuilder result = new StringBuilder(length);

      foreach (byte b in randomBytes)
      {
        result.Append(validChars[b % validChars.Length]);
      }

      return result.ToString();
    }
  }

}

public static class StringExtensions
{
  public static string RemoveSpacesBetweenCharacters(this string input)
  {
    // Use regular expression to replace all spaces between characters with an empty string.
    return Regex.Replace(input, @"(?<=\S)\s(?=\S)", "");
  }
}
