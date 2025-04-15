using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Helpers;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Common.Security;
using PMS.API.Application.DTOs.Common.Base.Response;
using PMS.API.Application.Services.Interfaces;
using PMS.API.Core.Domain.Entities.Identity;

namespace PMS.API.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommand : IRequest<ApplicationResult<long>>
{

  [Required]
  public required string FirstName { get; set; }

  [Required]
  public required string LastName { get; set; }

  [Required]
  public required string Email { get; set; }
  [Required]
  public required string Username { get; set; }

  public string? PhoneNumber { get; set; }
  public string? Address { get; set; }
}


public class CreateUserHandler : RequestHandlerBase<CreateUserCommand, ApplicationResult<long>>
{
  protected readonly IIdentityService _identityService;
  private readonly IEmailSenderService _emailService;
  private readonly IConfiguration _configuration;
  public CreateUserHandler(
  IIdentityService identityService,
    IEmailSenderService emailService,
    IConfiguration configuration,
    IServiceProvider serviceProvider,
    ILogger<CreateUserHandler> logger) : base(serviceProvider, logger)
  {
    _identityService = identityService;
    _emailService = emailService;
    _configuration = configuration;

  }

  protected override async Task<ApplicationResult<long>> HandleRequest(CreateUserCommand request, CancellationToken cancellationToken)
  {

    if (await _identityService.IsEmailExistAsync(request.Email!)) return ApplicationResult<long>.Error(new List<ValidationError>
    {
      new ValidationError{Name=nameof(request.Email),Message="Email already exist!"}
    });

    if (await _identityService.IsUserNameExistAsync(request.Username!)) return ApplicationResult<long>.Error(new List<ValidationError>
    {
      new ValidationError{Name=nameof(request.Username),Message="Username already exist!"}
    });

    // Add the User  Administrator
    var user = new User()
    {
      Email = request.Email,
      UserName = request.Username,
      FirstName = request.FirstName,
      LastName = request.LastName,
      PhoneNumber = request.PhoneNumber,
      UserType = Core.Enums.AppUserTypeEnums.Technician,
      Address = request.Address
    };

    var password = _identityService.GenerateRandomPassword();
    if (password.Length < 10)
    {
      password = _identityService.GenerateRandomPassword();
    }
    var resultUser = await _identityService.CreateUserAsync(user, password);

    if (resultUser.Success)
    {
      var newUser = await _identityService.GetByEmailAsync(request.Email!);
      if (newUser != null)
      {
        await _identityService.UpdateUserRole(newUser.Id!, _identityService.GetRoleByNameAsync(Roles.Technician)!.Result!.Id);
        EmailData data = new EmailData
        {
          ToEmail = request.Email!,
          ToName = $"{request.FirstName} {request.LastName}",
          Name = $"{request.FirstName} {request.LastName}",
          Email = request.Email!,
          Password = password,
          link = _configuration["UI:BaseURI"]
        };

        await _emailService.SendCredentialsEmail(data);

        return ApplicationResult<long>.SuccessResult(newUser.Id);
      }
    }
    return ApplicationResult<long>.Error("Error occurred while creating User");
  }
}
