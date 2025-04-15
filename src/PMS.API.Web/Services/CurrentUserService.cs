using System.Security.Claims;
using PMS.API.Application.Identity;
using PMS.API.Infrastructure.Interfaces;
using PMS.API.Web.Extensions;

namespace PMS.API.Web.Services;

public class CurrentUserService : DefaultCurrentUserService
{
  private readonly IHttpContextAccessor _httpContextAccessor;

  public CurrentUserService(IHttpContextAccessor httpContextAccessor)
  {
    _httpContextAccessor = httpContextAccessor;
  }

  public override long? UserId => _httpContextAccessor.GetClaimValue<long?>(IdentityConstants.UserIdClaim);
  public override string? Email => _httpContextAccessor.GetClaimValue<string?>(ClaimTypes.Email);
}
