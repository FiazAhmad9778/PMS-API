using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Auth.Commands.ChangePassword;
using PMS.API.Application.Features.Auth.Commands.Login;
using PMS.API.Application.Features.Auth.Commands.Logout;
using PMS.API.Application.Features.Auth.Commands.RefreshAuthToken;
using PMS.API.Application.Features.Auth.Commands.RegisterUser;
using PMS.API.Application.Features.Auth.Commands.ResetPassword;
using PMS.API.Application.Features.Auth.Commands.ResetPasswordRequest;
using PMS.API.Application.Features.Auth.Commands.VerifyUser;
using PMS.API.Application.Features.Auth.DTO;

namespace PMS.API.Web.Api;

[Route("api/[controller]")]
public class AuthController : BaseApiController
{
  public AuthController(IServiceProvider serviceProvider) : base(serviceProvider)
  {

  }

  [HttpPost("login")]
  [ProducesResponseType(typeof(ApplicationResult<AccountResultDto>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<AccountResultDto>> Login(LoginCommand command)
  {
    return await Mediator.Send(command);
  }

  [HttpPost("logout")]
  [ProducesResponseType(typeof(ApplicationResult<bool>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<bool>> Logout(LogoutCommand command)
  {
    return await Mediator.Send(command);
  }

  [HttpPost("register")]
  [AllowAnonymous]
  [ProducesResponseType(typeof(ApplicationResult<AccountResultDto>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<AccountResultDto>> Register([FromForm] RegisterCommand command)
  {
    return await Mediator.Send(command);
  }

  [HttpPost("refresh-token")]
  [AllowAnonymous]
  [ProducesResponseType(typeof(ApplicationResult<AccountResultDto>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<AccountResultDto>> RefreshToken(RefreshAuthTokenCommand command)
  {
    return await Mediator.Send(command);
  }

  [HttpPost("forgot-password")]
  [AllowAnonymous]
  [ProducesResponseType(typeof(ApplicationResult<ResetPasswordResultDto>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<ResetPasswordResultDto>> ResetPasswordRequest(ResetPasswordRequestCommand command)
  {
    return await Mediator.Send(command);
  }

  [HttpPost("reset-password")]
  [AllowAnonymous]
  [ProducesResponseType(typeof(ApplicationResult<ResetPasswordResultDto>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<ResetPasswordResultDto>> ResetPassword(ResetPasswordCommand command)
  {
    return await Mediator.Send(command);
  }

  [HttpPost("verify")]
  [ProducesResponseType(typeof(ApplicationResult<UserVerificationResultDto>), StatusCodes.Status200OK)]

  public async Task<ApplicationResult> VerifyUser(VerifyUserCommand command)
  {
    return await Mediator.Send(command);
  }

  [HttpPost("change-password")]
  [Authorize]
  [ProducesResponseType(typeof(ApplicationResult), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]

  public async Task<ApplicationResult> ChangePassword(ChangePasswordCommand command)
  {
    return await Mediator.Send(command);
  }
}
