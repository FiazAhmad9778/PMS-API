using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using PMS.API.Core.Domain.Entities.Identity;

namespace PMS.API.Application.Identity;
public class RoleClaimsAuthorizationHandler : AuthorizationHandler<RoleClaimsRequirement>
{
  private readonly UserManager<User> _userManager;

  public RoleClaimsAuthorizationHandler(UserManager<User> userManager)
  {
    _userManager = userManager;
  }

  protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleClaimsRequirement requirement)
  {
    var user = await _userManager.GetUserAsync(context.User);
    if (user == null)
    {
      context.Fail();
      return;
    }

    var roles = await _userManager.GetRolesAsync(user);
    if (roles == null || !roles.Any())
    {
      context.Fail();
      return;
    }

    foreach (var role in roles)
    {
      var claims = await _userManager.GetClaimsAsync(user);
      if (claims.Any(c => c.Type == requirement.ClaimType && c.Value == requirement.ClaimValue))
      {
        context.Succeed(requirement);
        return;
      }
    }

    context.Fail();
  }
}
public class RoleClaimsRequirement : IAuthorizationRequirement
{
  public string ClaimType { get; }
  public string ClaimValue { get; }

  public RoleClaimsRequirement(string claimType, string claimValue)
  {
    ClaimType = claimType;
    ClaimValue = claimValue;
  }
}
