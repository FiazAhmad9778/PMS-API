using System.ComponentModel.DataAnnotations;
using MediatR;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Auth.DTO;
using PMS.API.Application.Services.Interfaces;

namespace PMS.API.Application.Features.Auth.Commands.RefreshAuthToken;

public class RefreshAuthTokenCommand : IRequest<ApplicationResult<AccountResultDto>>
{
  [Required]
  public string? AccessToken { get; set; }
  [Required]
  public string? RefreshToken { get; set; }
}

public class RefreshAuthTokenCommandHandler :
    IRequestHandler<RefreshAuthTokenCommand, ApplicationResult<AccountResultDto>>
{
  private readonly IIdentityService _identityService;

  public RefreshAuthTokenCommandHandler(IIdentityService identityService)
  {
    _identityService = identityService;
  }

  public async Task<ApplicationResult<AccountResultDto>> Handle(RefreshAuthTokenCommand request,
      CancellationToken cancellationToken)
  {

    var result = await _identityService.RefreshToken(request.AccessToken!, request.RefreshToken!);
    return result;
  }
}
