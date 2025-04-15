namespace PMS.API.Web.Extensions;
using PMS.API.Application.Identity;
public static class HttpContextExtensions
{
  public static T? GetClaimValue<T>(this IHttpContextAccessor httpContextAccessor, string key)
  {
    if (httpContextAccessor.HttpContext?.User != null)
    {
      return httpContextAccessor.HttpContext!.User.GetValue<T>(key);
    }

    return default;
  }
}
