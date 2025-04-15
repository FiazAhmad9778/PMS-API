using System.ComponentModel.DataAnnotations;
using MediatR;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Auth.DTO;
using PMS.API.Application.Services.Interfaces;

namespace PMS.API.Application.Features.Auth.Commands.Login;

public class LoginCommand : IRequest<ApplicationResult<AccountResultDto>>
{
  [Required]
  public string? Email { get; set; }
  [Required]
  public string? Password { get; set; }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, ApplicationResult<AccountResultDto>>
{

  private readonly IIdentityService _identityService;

  public LoginCommandHandler(IIdentityService identityService)
  {
    _identityService = identityService;
  }

  public async Task<ApplicationResult<AccountResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
  {
    try
    {
      var loginResponse = await _identityService.SignInAsync(request.Email!, request.Password!);
      return loginResponse!;
    }
    catch (Exception ex)
    {
      return ApplicationResult<AccountResultDto>.Error(ex.Message);
    }
  }
}
