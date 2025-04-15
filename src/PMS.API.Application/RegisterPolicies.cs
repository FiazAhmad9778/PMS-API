using Microsoft.Extensions.DependencyInjection;
using PMS.API.Application.Identity;

namespace PMS.API.Application;
public static class RegisterPolicies
{
  public static void RegisterPolicy(this IServiceCollection services, IClaimApplication claimApplication)
  {
    services.AddAuthorization(options =>
    {
      var claimList = claimApplication.GetList();
      if (claimList.Any())
      {
        foreach (var item in claimList)
        {
          options.AddPolicy(item.ClaimCode!, policy => policy.RequireClaim(item.ClaimCode!));
        }
      }
    });
  }
}
