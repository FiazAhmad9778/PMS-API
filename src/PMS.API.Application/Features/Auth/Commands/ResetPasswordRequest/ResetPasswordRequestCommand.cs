using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.Extensions.Configuration;
using PMS.API.Application.Common.Extensions;
using PMS.API.Application.Common.Helpers;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Auth.DTO;
using PMS.API.Application.Services.Interfaces;

namespace PMS.API.Application.Features.Auth.Commands.ResetPasswordRequest;

public class ResetPasswordRequestCommand : IRequest<ApplicationResult<ResetPasswordResultDto>>
{
  [Required]
  [EmailAddress]
  public string? Email { get; set; }
  public string? Host { get; set; }
}

public class ResetPasswordRequestCommandHandler : IRequestHandler<ResetPasswordRequestCommand, ApplicationResult<ResetPasswordResultDto>>
{
  private readonly IIdentityService _identityService;
  private readonly IEmailSenderService _emailService;
  private readonly IConfiguration _configuration;

  public ResetPasswordRequestCommandHandler(
      IIdentityService identityService,
      IEmailSenderService emailService,
      IConfiguration configuration)
  {
    _identityService = identityService;
    _emailService = emailService;
    _configuration = configuration;
  }

  public async Task<ApplicationResult<ResetPasswordResultDto>> Handle(ResetPasswordRequestCommand request, CancellationToken cancellationToken)
  {
    var user = await _identityService.GetByEmailAsync(request.Email!);
    if (user == null)
    {
      return ApplicationResult<ResetPasswordResultDto>.Error("Account not Found");
    }
    var resetToken = await _identityService.GeneratePasswordResetTokenAsync(user);

    resetToken = GenerateResetTokenWithUserIdAsync(resetToken, user.Id);

    string urlLink;

    urlLink = $"{request.Host}/auth/reset-password?resetToken={resetToken}";

    EmailData data = new EmailData
    {
      ToEmail = request.Email!,
      ToName = $"{user.FirstName} {user.LastName}",
      Name = $"{user.FirstName} {user.LastName}",
      ResetLink = urlLink,
    };

    await _emailService.SendResetPasswordEmail(data);

    return ApplicationResult<ResetPasswordResultDto>.SuccessResult(new ResetPasswordResultDto() { });
  }

  private string GenerateResetTokenWithUserIdAsync(string resetToken, long userId)
  {
    var token = $"{userId}:{resetToken}";

    token = token.EncodeString();
    return token;
  }
}
