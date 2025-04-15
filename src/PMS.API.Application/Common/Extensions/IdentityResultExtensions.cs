using Microsoft.AspNetCore.Identity;

namespace PMS.API.Application.Common.Extensions;

public static class IdentityResultExtensions
{
  public static PMS.API.Application.Common.Models.ApplicationResult ToApplicationResult(this IdentityResult result)
  {
    return result.Succeeded
        ? PMS.API.Application.Common.Models.ApplicationResult.SuccessResult()
        : PMS.API.Application.Common.Models.ApplicationResult.Error(result.Errors.Select(e => e.Description));
  }
}
