using PMS.API.Core.Domain.Entities.Identity;

namespace PMS.API.Application.Services.Interfaces;
public interface IRoleService
{
  Task<bool> CreateRole(string roleName);
  Task<bool> CreateRole(string roleName, long clientGroupId);
  Task<bool> IsRoleExist(string roleName, long? roleId, long clientGroupId);
  Task<bool> DeleteRole(string roleName);
  Task<IEnumerable<Role>> GetRoles(long clientGroupId);
  Task<Role?> GetRoleById(long id);
  Task<Role?> FindByNameAsync(string roleName, long clientGroupId);
  Task<int> GetUserCountInRoleAsync(string roleName);
}
