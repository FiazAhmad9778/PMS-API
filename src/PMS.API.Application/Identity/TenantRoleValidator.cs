using Microsoft.AspNetCore.Identity;
using PMS.API.Core.Domain.Entities.Identity;

namespace PMS.API.Application.Identity;
// Reference Article
// https://stackoverflow.com/questions/41800273/duplicate-role-names-on-asp-net-identity-and-multi-tenancy
public class TenantRoleValidator : RoleValidator<Role>
{
  private IdentityErrorDescriber? Describer { get; set; }

  public TenantRoleValidator() : base()
  {

  }
  public override async Task<IdentityResult> ValidateAsync(RoleManager<Role> manager, Role role)
  {
    if (manager == null)
    {
      throw new ArgumentNullException(nameof(manager));
    }
    if (role == null)
    {
      throw new ArgumentNullException(nameof(role));
    }
    var errors = new List<IdentityError>();
    await ValidateRoleName(manager, role, errors);
    if (errors.Count > 0)
    {
      return IdentityResult.Failed(errors.ToArray());
    }
    return IdentityResult.Success;
  }
  private async Task ValidateRoleName(RoleManager<Role> manager, Role role,
  ICollection<IdentityError> errors)
  {
    var roleName = await manager.GetRoleNameAsync(role);
    if (string.IsNullOrWhiteSpace(roleName))
    {
      errors.Add(Describer!.InvalidRoleName(roleName));
    }
  }
}
