using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using PMS.API.Core.Domain.Entities.Identity;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Application.Identity;

public class ExtendedUserStore : UserStore<User, Role, AppDbContext, long, UserClaim, UserRole, UserLogin, UserToken, RoleClaim>
{

  public ExtendedUserStore(AppDbContext context,
      IdentityErrorDescriber? describer = null) : base(context, describer)
  {
  }


  public override async Task<bool> IsInRoleAsync(User user, string normalizedRoleName,
      CancellationToken cancellationToken = new CancellationToken())
  {
    //var applicationResult = await base.IsInRoleAsync(user, normalizedRoleName, cancellationToken);
    //return applicationResult;
    var roles = await GetRolesAsync(user, cancellationToken);
    if (roles.Any())
    {
      foreach (var role in roles)
      {
        if (role.ToLower().Equals(normalizedRoleName.ToLower()))
        {
          return true;
        }
      }
    }
    return false;
  }

  public override async Task<IList<string>> GetRolesAsync(User user,
      CancellationToken cancellationToken = new CancellationToken())
  {
    var roles = await base.GetRolesAsync(user, cancellationToken);
    return roles;
  }
}
