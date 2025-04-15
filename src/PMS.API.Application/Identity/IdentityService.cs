using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PMS.API.Application.Common.Exceptions;
using PMS.API.Application.Common.Extensions;
using PMS.API.Application.Common.Helpers;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Common.Security;
using PMS.API.Application.Features.Auth.DTO;
using PMS.API.Application.Services.Interfaces;
using PMS.API.Core.Domain.Entities.Identity;
using PMS.API.Core.Domain.Interfaces.Repositories;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Application.Identity;

public class IdentityService : IIdentityService
{
  private readonly UserManager<User> _userManager;
  private readonly AppDbContext _context;
  private readonly SignInManager<User> _signInManager;
  private readonly RoleManager<Role> _roleManager;
  private readonly IAuthorizationService _authorizationService;
  private readonly JwtOptions _jwtOptions;
  private readonly IConfiguration _configuration;
  private readonly IUserRepository _userRepository;

  public IdentityService(
    IConfiguration configuration,
      AppDbContext context,
      SignInManager<User> signInManager, UserManager<User> userManager,
      RoleManager<Role> roleManager,
      IUserRepository userRepository,
      IAuthorizationService authorizationService, IOptions<JwtOptions> jwtOptions)
  {
    _context = context;
    _signInManager = signInManager;
    _userManager = userManager;
    _roleManager = roleManager;
    _authorizationService = authorizationService;
    _jwtOptions = jwtOptions.Value;
    _configuration = configuration;
    _userRepository = userRepository;
  }

  public async Task<ApplicationResult<AccountResultDto>> SignInAsync(string email, string password)
  {
    var user = await _context.User.FirstOrDefaultAsync(x => x.NormalizedEmail == email.ToUpper() && !x.IsDeleted);


    if (user == null)
    {
      return ApplicationResult<AccountResultDto>.Error(Convert.ToString("Account not found"));
    }
    var userRoles = await _userManager.GetRolesAsync(user);
    var isValid = await _userManager.CheckPasswordAsync(user, password);

    if (isValid)
    {
      return await SignInUserAsync(user);
    }

    return ApplicationResult<AccountResultDto>.Error(Convert.ToString("Incorrect password"));
  }

  public async Task<ApplicationResult<AccountResultDto>> SignInAsync(long userId)
  {
    var user = await _context.User.FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted);

    if (user == null)
    {
      return ApplicationResult<AccountResultDto>.Error(Convert.ToString("Account not found"));
    }
    return await SignInUserWithIdAsync(user);
  }

  public async Task<ApplicationResult<AccountResultDto>> SignInUserWithIdAsync(User user)
  {
    try
    {
      if (user == null)
      {
        throw new NotFoundException("account Not Found");
      }

      List<string> claimList = new List<string>();

      //roles and claims
      var userRoles = await _userManager.GetRolesAsync(user);
      var userRole = _context.UserRole
                          .FirstOrDefault(x => x.UserId == user.Id);

      var claimsIdentity = new ClaimsIdentity(new Claim[]
                            {
                                    new Claim(ClaimTypes.Email, user.Email!),
                                    new Claim(IdentityConstants.UserIdClaim, user.Id.ToString()),
                                    new Claim(IdentityConstants.UserTypeIdClaim, Convert.ToString((int)user.UserType)),
                            });

      if (userRole != null)
      {
        var Role = _context.Roles.First(x => x.Id == userRole.RoleId);

        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, Role!.Name!));
        // Add the claim check here
        var roleClaims = _context
          .RoleClaim
          .Include(x => x.ApplicationClaim)
          .Where(x => x.RoleId == userRole.RoleId && x.IsAssigned)
          .ToList();

        foreach (var roleClaim in roleClaims)
        {
          claimsIdentity.AddClaim(new Claim(roleClaim.ApplicationClaim?.ClaimCode!, roleClaim.ApplicationClaim?.ClaimCode!));
          claimList.Add(roleClaim.ApplicationClaim?.ClaimCode!);
        }
      }

      var accessesToken = GenerateJWTAccessToken(user, claimsIdentity);
      var refreshToken = GenerateRefreshToken();
      // Add the new Refresh Token
      _context.UserTokens.Add(new UserToken
      {
        TokenExpiryTime = DateTime.Now.AddDays(30),
        UserId = user!.Id,
        Value = refreshToken,
        Name = user!.UserName!,
        LoginProvider = Guid.NewGuid().ToString(),
      });
      await _context.SaveChangesAsync();
      return ApplicationResult<AccountResultDto>.SuccessResult(new AccountResultDto()
      {
        AccessToken = accessesToken,
        RefreshToken = refreshToken,
        UserId = user.Id,
        Email = user.Email,
        Name = $"{user.FirstName} {user.LastName}",
        RoleName = userRoles!.Any() ? userRoles!.FirstOrDefault() : "",
        Claims = claimList
      });

    }
    catch (Exception ex)
    {

      return ApplicationResult<AccountResultDto>.Error("something went wrong with the user creation" + ex.Message);
    }
  }

  public async Task<ApplicationResult<bool>> SignOut()
  {
    await _signInManager.SignOutAsync();

    return ApplicationResult<bool>.SuccessResult(true);
  }

  public async Task<ApplicationResult<AccountResultDto>> SignInUserAsync(User user)
  {
    try
    {
      if (user == null)
      {
        throw new NotFoundException("account Not Found");
      }

      //roles and claims
      List<string> claimList = new List<string>();
      var userRoles = await _userManager.GetRolesAsync(user);
      var userRole = _context.UserRole
                          .FirstOrDefault(x => x.UserId == user.Id);

      var claimsIdentity = new ClaimsIdentity(new Claim[]
          {
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(IdentityConstants.UserIdClaim, user.Id.ToString()),
                new Claim(IdentityConstants.UserTypeIdClaim, Convert.ToString((int)user.UserType)),
          });

      if (userRole != null)
      {
        var Role = _context.Roles.First(x => x.Id == userRole.RoleId);
        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, Role!.Name!));
        // Add the claim check here
        var roleClaims = _context
          .RoleClaim
          .Include(x => x.ApplicationClaim)
          .Where(x => x.RoleId == userRole.RoleId && x.IsAssigned)
          .ToList();

        foreach (var roleClaim in roleClaims)
        {
          claimsIdentity.AddClaim(new Claim(roleClaim.ApplicationClaim?.ClaimCode!, roleClaim.ApplicationClaim?.ClaimCode!));
          claimList.Add(roleClaim.ApplicationClaim?.ClaimCode!);
        }
      }

      var accessesToken = GenerateJWTAccessToken(user, claimsIdentity);
      var refreshToken = GenerateRefreshToken();
      // Add the new Refresh Token
      _context.UserTokens.Add(new UserToken
      {
        TokenExpiryTime = DateTime.Now.AddMinutes(30),
        UserId = user!.Id,
        Value = refreshToken,
        Name = user!.UserName!,
        LoginProvider = Guid.NewGuid().ToString(),
      });
      await _context.SaveChangesAsync();
      return ApplicationResult<AccountResultDto>.SuccessResult(new AccountResultDto()
      {
        AccessToken = accessesToken,
        RefreshToken = refreshToken,
        UserId = user.Id,
        Email = user.Email,
        Name = $"{user.FirstName} {user.LastName}",
        RoleName = userRoles!.Any() ? userRoles!.FirstOrDefault() : "",
        ProfileImage = user.AvatarUrl,
        Claims = claimList
      });

    }
    catch (Exception ex)
    {

      return ApplicationResult<AccountResultDto>.Error("something went wrong with the user creation" + ex.Message);
    }
  }

  public async Task<ApplicationResult<AccountResultDto>> RefreshToken(string accessToken, string refreshToken)
  {
    var principal = GetPrincipalFromExpiredToken(accessToken!);
    if (principal == null)
    {
      throw new ForbiddenAccessException();
    }

    var claims = principal.Identities!.SelectMany(x => x.Claims);
    string userId = claims!.FirstOrDefault(x => x.Type == "UserId")!.Value;
    var user = await GetById(Convert.ToInt64(userId));
    var userRefreshToken = await _context.UserTokens.FirstOrDefaultAsync(x => x.UserId == user!.Id && x.Value == refreshToken);
    if (user == null || userRefreshToken == null || userRefreshToken.TokenExpiryTime <= DateTime.UtcNow)
    {
      return ApplicationResult<AccountResultDto>.Error("Invalid access token or refresh token");
    }
    List<string> claimList = new List<string>();
    //roles and claims
    var userRoles = await _userManager.GetRolesAsync(user);
    var userRole = _context.UserRole
                        .FirstOrDefault(x => x.UserId == user.Id);

    var claimsIdentity = new ClaimsIdentity(new Claim[]
                              {
                                    new Claim(ClaimTypes.Email, user.Email!),
                                    new Claim(IdentityConstants.UserIdClaim, user.Id.ToString()),
                                    new Claim(IdentityConstants.UserTypeIdClaim, Convert.ToString((int)user.UserType)),
                              });
    if (userRole != null)
    {
      var Role = _context.Roles.First(x => x.Id == userRole.RoleId);
      claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, Role!.Name!));
      // Add the claim check here
      var roleClaims = _context
        .RoleClaim
        .Include(x => x.ApplicationClaim)
        .Where(x => x.RoleId == userRole.RoleId && x.IsAssigned)
        .ToList();

      foreach (var roleClaim in roleClaims)
      {
        claimsIdentity.AddClaim(new Claim(roleClaim.ApplicationClaim?.ClaimCode!, roleClaim.ApplicationClaim?.ClaimCode!));
        claimList.Add(roleClaim.ApplicationClaim?.ClaimCode!);
      }
    }
    var newAccessToken = GenerateJWTAccessToken(user!, claimsIdentity!);
    var newRefreshToken = GenerateRefreshToken();
    // Add the new Refresh Token
    _context.UserTokens.Add(new UserToken
    {
      //TokenExpiryTime = DateTime.Now.AddMinutes(_jwtOptions.ExpireAfterMinute),
      TokenExpiryTime = DateTime.Now.AddDays(30),
      UserId = user!.Id,
      Value = newRefreshToken,
      Name = user!.Email!,
      LoginProvider = Guid.NewGuid().ToString(),
    });
    await _context.SaveChangesAsync();
    return ApplicationResult<AccountResultDto>.SuccessResult(new AccountResultDto()
    {
      AccessToken = newAccessToken,
      RefreshToken = newRefreshToken,
      UserId = user.Id,
      Email = user.Email,
      Name = $"{user.FirstName} {user.LastName}",
      RoleName = userRoles!.Any() ? userRoles!.FirstOrDefault() : "",
      Claims = claimList,
      ProfileImage = user.AvatarUrl
    });
  }

  public async Task<ApplicationResult> CreateUserAsync(User user, string? password = null)
  {
    IdentityResult result;
    if (!string.IsNullOrEmpty(password))
    {
      result = await _userManager.CreateAsync(user, password);
    }
    else
    {
      result = await _userManager.CreateAsync(user);
    }

    return result.ToApplicationResult();
  }

  public async Task<ApplicationResult> AssignRoleAsync(User user, string role)
  {
    foreach (var checkRole in Roles.Application)
    {
      if (_roleManager.Roles.All(_ => _.Name != checkRole))
      {
        await _roleManager.CreateAsync(new Role(checkRole));
      }
    }

    try
    {

      var result = await _userManager.AddToRoleAsync(user, role);
      return result.ToApplicationResult();
    }
    catch (Exception ex)
    {

      var heheh = ex;

      throw new Exception(heheh.Message);
      //throw ex;
    }
  }

  public async Task<User> GetById(long id)
  {
    var user = await _userManager.FindByIdAsync(id.ToString());
    if (user == null)
    {
      throw new NotFoundException();
    }
    return user;
  }

  public async Task<string> GetUserNameAsync(long userId)
  {
    var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
    if (user == null)
    {
      return "";
    }

    return user?.UserName!;
  }

  public async Task<bool> VerifyUserViaToken(User user, string token)
  {
    if (string.IsNullOrEmpty(token))
    {
      return false;
    }
    else
    {
      var confirmedUser = await _userManager.ConfirmEmailAsync(user, token);
      return confirmedUser?.Succeeded ?? false;
    }
  }

  public async Task<bool> IsInRoleAsync(long userId, string role)
  {
    var user = _userManager.Users.SingleOrDefault(u => u.Id == userId);


    return user != null && await _userManager.IsInRoleAsync(user, role);
  }

  public async Task<bool> AuthorizeAsync(long userId, string policyName)
  {
    var user = _userManager.Users.SingleOrDefault(u => u.Id == userId);

    if (user == null)
    {
      return false;
    }

    var principal = await _signInManager.CreateUserPrincipalAsync(user);

    var result = await _authorizationService.AuthorizeAsync(principal, policyName);

    return result.Succeeded;
  }

  private string GenerateJWTAccessToken(User user, ClaimsIdentity claimsIdentity)
  {
    return new JwtTokenBuilder()
            .WithOption(_jwtOptions)
            .WithClaimsIdentity(claimsIdentity)
            .Build();
  }

  private async Task<string> GenerateAccessToken(User user)
  {

    var principal = await _signInManager.CreateUserPrincipalAsync(user);

    return new JwtTokenBuilder()
        .WithOption(_jwtOptions)
        .WithClaimsPrincipal(principal)
        .Build();
  }
  public ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
  {
    var tokenValidationParameters = new TokenValidationParameters
    {
      ValidateAudience = false,
      ValidateIssuer = false,
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration!.GetValue<string>("JwtOptions:Secret")!)),
      ValidateLifetime = false
    };

    var tokenHandler = new JwtSecurityTokenHandler();
    var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
    if (securityToken == null || principal == null)
      throw new SecurityTokenException("Invalid token");

    return principal;
  }
  private string GenerateRefreshToken()
  {
    var randomNumber = new byte[64];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(randomNumber);
    return Convert.ToBase64String(randomNumber);
  }

  public async Task<User?> GetByEmailAsync(string emailAddress)
  {
    if (string.IsNullOrEmpty(emailAddress)) return null;
    return await _context.User.FirstOrDefaultAsync(x => x.NormalizedEmail == emailAddress.ToUpper() && !x.IsDeleted);
  }
  public async Task<bool> IsEmailExistAsync(string emailAddress)
  {
    if (string.IsNullOrEmpty(emailAddress)) return false;
    return (await _context.User.FirstOrDefaultAsync(x => x.NormalizedEmail == emailAddress.ToUpper() && !x.IsDeleted) != null);
  }

  public async Task<bool> IsUserNameExistAsync(string username)
  {
    if (string.IsNullOrEmpty(username)) return false;
    return (await _context.User.FirstOrDefaultAsync(x => x.NormalizedUserName == username.ToUpper() && !x.IsDeleted) != null);
  }

  public async Task<string> GeneratePasswordResetTokenAsync(User user)
  {
    if (user == null) return "";
    return await _userManager.GeneratePasswordResetTokenAsync(user);
  }

  public Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword)
  {
    return _userManager.ResetPasswordAsync(user, token, newPassword);
  }

  public async Task<Role?> GetRoleByNameAsync(string roleName)
  {
    return await _roleManager.FindByNameAsync(roleName);
  }

  public async Task<Role?> CreateRoleAsync(string roleName)
  {
    if (string.IsNullOrEmpty(roleName)) return null;
    await _roleManager.CreateAsync(new Role(roleName));
    return await GetRoleByNameAsync(roleName);
  }

  public async Task<string> GetCompanyConfirmationLink(User user)
  {
    return await _userManager.GenerateEmailConfirmationTokenAsync(user);
  }

  public async Task<bool> UpdateUserInformation(User user)
  {
    var result = await _userManager.UpdateAsync(user);
    return result.Succeeded;
  }

  public async Task<IList<Role>> GetRoles()
  {
    return await _roleManager.Roles.ToListAsync();
  }
  public async Task<IList<User>> GetAllUsers()
  {
    return await _userManager.Users.ToListAsync();
  }

  public async Task<bool> UpdateUserDetails(User userDetails)
  {
    var user = await _userManager.FindByIdAsync(userDetails.Id.ToString());

    if (user == null)
    {
      return false;
    }

    user.FirstName = userDetails.FirstName;
    user.LastName = userDetails.LastName;
    user.PhoneNumber = userDetails.PhoneNumber;
    user.AvatarUrl = !string.IsNullOrEmpty(userDetails.AvatarUrl) ? userDetails.AvatarUrl : user.AvatarUrl;
    var result = await _userManager.UpdateAsync(user);

    return result.Succeeded;
  }
  public async Task<bool> UpdateUserWarehouseId(User userDetails)
  {
    var user = await _userManager.FindByIdAsync(userDetails.Id.ToString());

    if (user == null)
    {
      return false;
    }

    var result = await _userManager.UpdateAsync(user);

    return result.Succeeded;
  }

  public async Task<bool> UpdateUserRole(long userId, long newRoleId)
  {
    var user = await _userManager.FindByIdAsync(userId.ToString());

    if (user == null)
    {
      return false; // User not found
    }

    // Get the current role for the user
    var currentRole = await _userManager.GetRolesAsync(user);
    var role = await _roleManager.FindByIdAsync(newRoleId.ToString());
    if (role != null)
    {
      if (currentRole != null && currentRole.Count == 1 && currentRole[0] != role.Name)
      {
        // Remove the user from the current role
        await _userManager.RemoveFromRoleAsync(user, currentRole[0]);
      }
      //await _userManager.AddToRoleAsync(user, role.Name!);
      await _context.UserRole.AddAsync(new UserRole
      {
        UserId = user.Id,
        RoleId = newRoleId
      });
      await _context.SaveChangesAsync();
    }


    // Assign the new role to the user
    return true;
  }

  public async Task<IList<Role>> GetUserRolesAsync(User user)
  {
    var roleNames = await _userManager.GetRolesAsync(user);
    var roles = new List<Role>();

    foreach (var roleName in roleNames)
    {
      var role = await _roleManager.FindByNameAsync(roleName);
      if (role != null)
      {
        roles.Add(role);
      }
    }
    return roles;
  }

  public async Task<ApplicationResult> ChangePassword(long userId, string currentPassword, string newPassword)
  {
    if (currentPassword.ToLower().Equals(newPassword.ToLower()))
    {
      return PMS.API.Application.Common.Models.ApplicationResult.Error(new[] { "You cannot update password to the current password" });
    }
    var user = await GetById(userId);
    var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
    return result.ToApplicationResult();
  }
  public string GenerateRandomPassword()
  {
    return RandomGeneratorUtil.GenerateRandomPassword(12);
  }
}
