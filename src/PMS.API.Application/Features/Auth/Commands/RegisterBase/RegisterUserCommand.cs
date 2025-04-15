using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using PMS.API.Application.Common.Helpers;
using PMS.API.Application.Common.Models;
using PMS.API.Application.DTOs.Common.Base.Response;
using PMS.API.Application.Services.Interfaces;
using PMS.API.Core.Domain.Entities.Identity;
using PMS.API.Core.Enums;

namespace PMS.API.Application.Features.Auth.Commands.RegisterBase;

public abstract class RegisterUserCommand
{
  public required string FirstName { get; set; }
  public required string LastName { get; set; }
  [Required]
  public string? Email { get; set; }
  [Required]
  public string? Password { get; set; }
  public string? PhoneNumber { get; set; }
  public IFormFile? ProfileImage { get; set; }
  public AppUserTypeEnums UserType { get; set; }
}

public abstract class RegisterUserCommandHandler
{
  protected readonly IIdentityService _identityService;
  protected readonly IFileUploadService _fileUploadService;

  protected RegisterUserCommandHandler(
    IIdentityService identityService,
    IFileUploadService fileUploadService)
  {
    _identityService = identityService;
    _fileUploadService = fileUploadService;
  }

  protected async Task<ApplicationResult<User>> RegisterUser(RegisterUserCommand request)
  {

    if (await _identityService.IsEmailExistAsync(request.Email!)) return ApplicationResult<User>.Error(new List<ValidationError>
    {
      new ValidationError{Name=nameof(request.Email),Message="A User already registered with this email."}
    });

    var user = new User()
    {
      Email = request.Email,
      UserName = request.Email,
      FirstName = request.FirstName,
      LastName = request.LastName,
      PhoneNumber = request.PhoneNumber,
      UserType = request.UserType,
    };

    if (request.ProfileImage != null)
    {
      // Check if the property is not already filled
      var imageUpload = await _fileUploadService.UploadImage(request.ProfileImage);

      if (!imageUpload.result)
        return ApplicationResult<User>.Error(new ValidationError { Name = nameof(request.ProfileImage), Message = "" });

      user.AvatarUrl = imageUpload.ImageUrl;
    }

    var result = await _identityService.CreateUserAsync(user, request.Password);

    if (result.Success)
    {
      var createdUser = await _identityService.GetByEmailAsync(request.Email!);
      return ApplicationResult<User>.SuccessResult(createdUser!);
    }
    return ApplicationResult<User>.Error(result.ValidationErrors!.FirstOrDefault()!.Message);

  }
}
