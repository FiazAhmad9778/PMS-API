using MediatR;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Auth.DTO;
using PMS.API.Application.Services.Interfaces;

namespace PMS.API.Application.Features.Auth.Commands.Login;

public class GetTokenByRoleCommand : IRequest<ApplicationResult<AccountResultDto>>
{
  public bool IsContractAdmin { get; set; }
}

public class GetTokenByRoleCommandHandler : RequestHandlerBase, IRequestHandler<GetTokenByRoleCommand, ApplicationResult<AccountResultDto>>
{

  private readonly IIdentityService _identityService;

  public GetTokenByRoleCommandHandler(
    IIdentityService identityService,
    IServiceProvider serviceProvider,
    ILogger<GetTokenByRoleCommandHandler> logger
    ) : base(serviceProvider, logger)
  {
    _identityService = identityService;
  }

  public async Task<ApplicationResult<AccountResultDto>> Handle(GetTokenByRoleCommand request, CancellationToken cancellationToken)
  {
    try
    {
      var loginResponse = await _identityService.SignInAsync(_currentUser.Id);
      return loginResponse!;
    }
    catch (Exception ex)
    {
      return ApplicationResult<AccountResultDto>.Error(ex.Message);
    }
  }
}
