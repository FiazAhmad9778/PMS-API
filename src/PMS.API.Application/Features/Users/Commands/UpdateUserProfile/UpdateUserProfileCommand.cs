using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Services.Interfaces;
using PMS.API.Core.Domain.Interfaces.Repositories;

namespace PMS.API.Application.Features.Users.Commands.UpdateUserProfile;
public class UpdateUserProfileCommand : IRequest<ApplicationResult<long>>
{
  public required string FirstName { get; set; }
  public required string LastName { get; set; }
  public string? PhoneNumber { get; set; }
  public IFormFile? ProfileImage { get; set; }
  public string? SignatureBase64 { get; set; }
  public string? Address { get; set; }
}

public class UpdateUserHandler : RequestHandlerBase<UpdateUserProfileCommand, ApplicationResult<long>>
{
  private readonly IUserRepository _UserRepository;
  private readonly IIdentityService _identityService;
  public UpdateUserHandler(
    IUserRepository UserRepository,
    IServiceProvider serviceProvider,
    ILogger<UpdateUserHandler> logger,
    IIdentityService identityService) : base(serviceProvider, logger)
  {
    _UserRepository = UserRepository;
    _identityService = identityService;
  }

  protected override async Task<ApplicationResult<long>> HandleRequest(UpdateUserProfileCommand request, CancellationToken cancellationToken)
  {
    var existingUser = await _UserRepository.FirstAsync(x => x.Id == _currentUser.Id);
    if (existingUser == null) return ApplicationResult<long>.Error("User not found!");

    if (!string.IsNullOrEmpty(request.FirstName))
    {
      existingUser.FirstName = request.FirstName;
    }
    if (!string.IsNullOrEmpty(request.LastName))
    {
      existingUser.LastName = request.LastName;
    }
    if (!string.IsNullOrEmpty(request.PhoneNumber))
    {
      existingUser.PhoneNumber = request.PhoneNumber;
    }
    if (!string.IsNullOrEmpty(request.Address))
    {
      existingUser.Address = request.Address;
    }

    if (request.SignatureBase64 != null)
    {
      byte[] signatureBytes = Convert.FromBase64String(request.SignatureBase64);
      existingUser.SignatureData = signatureBytes;
    }
    else
    {
      existingUser.SignatureData = null;
    }

    var updateUserResult = await _identityService.UpdateUserInformation(existingUser);

    if (updateUserResult)
      return ApplicationResult<long>.SuccessResult(existingUser.Id);
    return ApplicationResult<long>.Error("Error occurred while updating User");
  }
}
