using Microsoft.AspNetCore.Identity;

namespace PMS.API.Core.Domain.Entities.Identity;

public class UserToken : IdentityUserToken<long>
{
  public DateTime? TokenExpiryTime { get; set; }
}
