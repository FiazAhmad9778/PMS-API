using MediatR;
using PMS.API.Application.Common.Helpers;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Common.Security;
using PMS.API.Application.Features.Auth.Commands.Login;
using PMS.API.Application.Features.Auth.Commands.RegisterBase;
using PMS.API.Application.Features.Auth.DTO;
using PMS.API.Application.Services.Interfaces;

namespace PMS.API.Application.Features.Auth.Commands.RegisterUser;


public class RegisterCommand : RegisterUserCommand, IRequest<ApplicationResult<AccountResultDto>>
{
  public string? FcmToken { get; set; }
  public string? DeviceId { get; set; }
  public string? Platform { get; set; }
}

public class RegisterCompanyUserCommandHandler : RegisterUserCommandHandler,
    IRequestHandler<RegisterCommand, ApplicationResult<AccountResultDto>>
{
  public RegisterCompanyUserCommandHandler(
    IIdentityService identityService,
    IFileUploadService fileUploadService) :
      base(identityService, fileUploadService)
  {
  }

  public async Task<ApplicationResult<AccountResultDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
  {
    var result = await RegisterUser(request);

    if (result.Success)
    {

      {
        await _identityService.AssignRoleAsync(result.Data!, Roles.Technician);

        var loginCommand = new LoginCommand
        {
          Email = request.Email,
          Password = request.Password,
        };

        var loginResponse = await _identityService.SignInAsync(request.Email!, request.Password!);
        return loginResponse!;
      }
    }

    return result.ValidationErrors.Any() ? ApplicationResult<AccountResultDto>.Error(result.ValidationErrors!) : result.Errors.Any() ? ApplicationResult<AccountResultDto>.Error(result.Errors!) : ApplicationResult<AccountResultDto>.Error(result.Message!);
  }
}
