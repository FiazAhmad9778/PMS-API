using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Auth.DTO;
using PMS.API.Core.Domain.Entities.Identity;

namespace PMS.API.Application.Services.Interfaces;

public interface IIdentityService
{
  Task<ApplicationResult<AccountResultDto>> SignInAsync(string email, string password);

  Task<ApplicationResult<AccountResultDto>> SignInUserAsync(User user);

  Task<ApplicationResult<AccountResultDto>> RefreshToken(string accessToken, string refreshToken);

  Task<ApplicationResult> CreateUserAsync(User user, string? password = null);
  ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token);
  Task<ApplicationResult> AssignRoleAsync(User user, string role);

  Task<User> GetById(long id);

  Task<User?> GetByEmailAsync(string emailAddress);
  Task<bool> IsEmailExistAsync(string emailAddress);

  Task<bool> VerifyUserViaToken(User user, string token);
  Task<string> GetCompanyConfirmationLink(User user);
  Task<string> GetUserNameAsync(long userId);
  Task<string> GeneratePasswordResetTokenAsync(User user);
  Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword);
  Task<bool> IsInRoleAsync(long userId, string role);

  Task<bool> AuthorizeAsync(long userId, string policyName);
  Task<Role?> GetRoleByNameAsync(string roleName);
  Task<Role?> CreateRoleAsync(string roleName);

  Task<bool> UpdateUserInformation(User user);
  Task<IList<Role>> GetRoles();
  Task<IList<User>> GetAllUsers();
  string GenerateRandomPassword();
  Task<ApplicationResult> ChangePassword(long userId, string currentPassword, string newPassword);
  Task<bool> UpdateUserDetails(User userDetails);
  Task<bool> UpdateUserWarehouseId(User userDetails);
  Task<bool> UpdateUserRole(long userId, long newRoleId);
  Task<IList<Role>> GetUserRolesAsync(User user);
  Task<ApplicationResult<bool>> SignOut();
  Task<ApplicationResult<AccountResultDto>> SignInAsync(long userId);
  Task<bool> IsUserNameExistAsync(string username);
}
