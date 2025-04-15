using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using PMS.API.Application.Common.Extensions;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Auth.DTO;
using PMS.API.Application.Services.Interfaces;

namespace PMS.API.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommand : IRequest<ApplicationResult<ResetPasswordResultDto>>
{
  [Required]
  public string? Token { get; set; }
  [Required]
  public string? Password { get; set; }
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ApplicationResult<ResetPasswordResultDto>>
{
  private readonly IDataProtector _dataProtector;
  private readonly IIdentityService _identityService;
  public ResetPasswordCommandHandler(IDataProtectionProvider dataProtectionProvider, IIdentityService identityService)
  {
    _dataProtector = dataProtectionProvider.CreateProtector(new DataProtectionTokenProviderOptions().Name);
    _identityService = identityService;
  }
  public async Task<ApplicationResult<ResetPasswordResultDto>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
  {

    request.Token = request.Token!.DecodeString();

    // Split the token to extract user ID and unique identifier
    var tokenParts = request.Token!.Split(':');

    if (tokenParts.Length != 2)
    {
      return ApplicationResult<ResetPasswordResultDto>.Error("Token is Invalid!");
    }

    var userId = Convert.ToInt64(tokenParts[0]);
    var resetPasswordToken = tokenParts[1];


    var user = await _identityService.GetById(userId);
    if (user == null)
    {
      return ApplicationResult<ResetPasswordResultDto>.Error("Token is Invalid!");
    }

    var result = await _identityService.ResetPasswordAsync(user, resetPasswordToken, request.Password!);
    if (!result.Succeeded)
    {
      return ApplicationResult<ResetPasswordResultDto>.Error("Token is Invalid!");
    }

    return ApplicationResult<ResetPasswordResultDto>.SuccessResult(new ResetPasswordResultDto() { });
  }

}

