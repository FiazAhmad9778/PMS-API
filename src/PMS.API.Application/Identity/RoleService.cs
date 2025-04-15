using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PMS.API.Application.Services.Interfaces;
using PMS.API.Core.Domain.Entities.Identity;

namespace PMS.API.Application.Identity;

public class RoleService : IRoleService
{
  private readonly RoleManager<Role> _roleManager;
  private readonly UserManager<User> _userManager;
  public RoleService(RoleManager<Role> roleManager, UserManager<User> userManager)
  {
    _roleManager = roleManager;
    _userManager = userManager;
  }

  public async Task<bool> CreateRole(string roleName)
  {
    var role = new Role(roleName);
    var result = await _roleManager.CreateAsync(role);
    return result.Succeeded;
  }
  public async Task<bool> CreateRole(string roleName, long clientGroupId)
  {
    var role = new Role(roleName, clientGroupId);
    var result = await _roleManager.CreateAsync(role);
    return result.Succeeded;
  }
  public async Task<IEnumerable<Role>> GetRoles(long clientGroupId)
  {
    return await _roleManager.Roles.Where(x => (clientGroupId == 0 || x.ClientGroupId == clientGroupId) && !x.IsSystem).ToListAsync();
  }
  public async Task<bool> IsRoleExist(string roleName, long? roleId, long clientGroupId)
  {
    var role = await FindByNameAsync(roleName, clientGroupId);
    if (role != null && roleId.HasValue && role.Id == roleId && role.ClientGroupId == clientGroupId)
    {
      role = null;
    }
    else if (role != null && role.ClientGroupId != clientGroupId)
    {
      role = null;
    }
    return role != null;
  }
  public async Task<bool> DeleteRole(string roleName)
  {
    var role = await _roleManager.FindByNameAsync(roleName);
    if (role == null)
      return false;

    var result = await _roleManager.DeleteAsync(role);
    return result.Succeeded;
  }

  public async Task<Role?> GetRoleById(long id)
  {
    return await _roleManager.FindByIdAsync($"{id}");
  }

  public async Task<Role?> FindByNameAsync(string roleName, long clientGroupId)
  {
    roleName = roleName.ToLower();
    return await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name!.ToLower().Equals(roleName) && r.ClientGroupId == clientGroupId);
  }
  public async Task<int> GetUserCountInRoleAsync(string roleName)
  {
    var role = await _roleManager.FindByNameAsync(roleName);
    if (role == null)
    {
      return 0;
    }

    var users = _userManager.Users.ToList();
    int count = 0;
    foreach (var user in users)
    {
      if (await _userManager.IsInRoleAsync(user, roleName))
      {
        count++;
      }
    }

    return count;
  }
}
