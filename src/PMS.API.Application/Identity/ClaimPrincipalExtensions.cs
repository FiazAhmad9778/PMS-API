using System.Security.Claims;

namespace PMS.API.Application.Identity;

public static class ClaimPrincipalExtensions
{
  public static T? GetValue<T>(this ClaimsPrincipal? principal, string key)
  {
    var claim = principal?.FindFirstValue(key);
    if (!string.IsNullOrEmpty(claim))
    {
      var nullableType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
      return (T)Convert.ChangeType(claim, nullableType);
    }

    return default;
  }
}
