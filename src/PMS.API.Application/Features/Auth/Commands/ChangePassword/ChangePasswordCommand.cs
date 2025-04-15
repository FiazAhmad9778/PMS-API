using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Security;
using PMS.API.Application.Services.Interfaces;
using ApplicationResult = PMS.API.Application.Common.Models.ApplicationResult;

namespace PMS.API.Application.Features.Auth.Commands.ChangePassword;

[AuthorizeRequest]
public class ChangePasswordCommand : IRequest<ApplicationResult>
{
  [Required]
  public string? CurrentPassword { get; set; }
  [Required]
  public string? NewPassword { get; set; }
}

public class ChangePasswordCommandHandler : RequestHandlerBase<ChangePasswordCommand, ApplicationResult>
{
  private readonly IIdentityService _identityService;

  public ChangePasswordCommandHandler(IIdentityService identityService, IServiceProvider serviceProvider,
    ILogger<ChangePasswordCommandHandler> logger) : base(serviceProvider, logger)
  {
    _identityService = identityService;
  }

  protected override async Task<ApplicationResult> HandleRequest(ChangePasswordCommand request, CancellationToken cancellationToken)
  {
    return await _identityService.ChangePassword(_currentUser.Id, request.CurrentPassword!,
        request.NewPassword!);
  }
}
