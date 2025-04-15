using System.Text;

namespace PMS.API.Application.Common.Extensions;

public static class StringExtensions
{
  public static string EncodeString(this string plainText)
  {
    var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
    var encodedText = Convert.ToBase64String(plainTextBytes);
    return encodedText;
  }
  public static string DecodeString(this string encodedText)
  {
    var encodedTextBytes = Convert.FromBase64String(encodedText);
    var plainText = Encoding.UTF8.GetString(encodedTextBytes);
    return plainText;
  }
  public static string GetInitial(this string name)
  {
    if (string.IsNullOrEmpty(name))
    {
      return "";
    }

    // Take the first character, convert to uppercase, and return
    return char.ToUpper(name[0]).ToString();
  }
  public static string GetEnumDisplayName(this Enum value)
  {
    string name = value.ToString();
    string displayName = string.Empty;
    if (string.IsNullOrEmpty(name)) return "";
    foreach (char c in name)
    {
      if (char.IsUpper(c) && displayName.Length > 0)
      {
        displayName += " ";
      }
      displayName += c;
    }

    return displayName;
  }
}
