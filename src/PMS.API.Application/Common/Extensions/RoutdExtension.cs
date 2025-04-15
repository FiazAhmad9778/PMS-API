using System.Text;

namespace PMS.API.Application.Common.Extensions;

public static class PMSExtension
{
  public static string FormatToPhoneNumber(this string phoneNumber)
  {
    if (!string.IsNullOrEmpty(phoneNumber))
    {
      var tempPhone = phoneNumber.Replace("-", string.Empty);
      if (tempPhone.Length >= 10)
      {
        var phone = new StringBuilder();

        phone.Append(tempPhone.Substring(0, 3)).Append("-");
        phone.Append(tempPhone.Substring(3, 3)).Append("-");
        phone.Append(tempPhone.Substring(6, 4));

        return phone.ToString();
      }
    }

    return phoneNumber;
  }
  public static DateTime? ConvertToGMTStandardTime(this DateTime? utc)
  {
    try
    {
      if (!utc.HasValue)
        return null;

      TimeZoneInfo gmtTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
      return TimeZoneInfo.ConvertTimeFromUtc(utc!.Value, gmtTimeZone);
    }
    catch
    {
      return null;
    }
  }
}
