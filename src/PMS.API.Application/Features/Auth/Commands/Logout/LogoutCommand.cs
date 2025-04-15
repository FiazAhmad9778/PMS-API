using MediatR;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Services.Interfaces;
using PMS.API.Infrastructure.Interfaces;

namespace PMS.API.Application.Features.Auth.Commands.Logout;

public class LogoutCommand : IRequest<ApplicationResult<bool>>
{
  public string? FcmToken { get; set; }
  public string? DeviceId { get; set; }
  public string? Platform { get; set; }
}

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, ApplicationResult<bool>>
{

  private readonly IIdentityService _identityService;
  private readonly ICurrentUserService _currentUserService;
  public LogoutCommandHandler(
    IIdentityService identityService,
    ICurrentUserService currentUserService)
  {
    _identityService = identityService;
    _currentUserService = currentUserService;
  }

  public async Task<ApplicationResult<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
  {

    try
    {
      return await _identityService.SignOut();
    }
    catch (Exception ex)
    {
      return ApplicationResult<bool>.Error(ex.Message);
    }
  }
}
