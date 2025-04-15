using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Services.Interfaces;
using PMS.API.Core.Domain.Entities.Identity;
using PMS.API.Core.Domain.Interfaces.Repositories;

namespace PMS.API.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommand : IRequest<ApplicationResult<long>>
{
  public long Id { get; set; }

  [Required]
  public required string FirstName { get; set; }

  [Required]
  public required string LastName { get; set; }
  public string? PhoneNumber { get; set; }
  public string? Address { get; set; }
}

public class UpdateUserHandler : RequestHandlerBase<UpdateUserCommand, ApplicationResult<long>>
{
  private readonly IUserRepository _UserRepository;
  private readonly IIdentityService _identityService;

  public UpdateUserHandler(IUserRepository UserRepository,
    IServiceProvider serviceProvider,
    ILogger<UpdateUserHandler> logger,
    IIdentityService identityService) : base(serviceProvider, logger)
  {
    _UserRepository = UserRepository;
    _identityService = identityService;
  }

  protected override async Task<ApplicationResult<long>> HandleRequest(UpdateUserCommand request, CancellationToken cancellationToken)
  {
    var existingUser = await _UserRepository.FirstAsync(x => x.Id == request.Id);
    if (existingUser == null) return ApplicationResult<long>.Error("User not found!");

    var user = new User()
    {
      Id = existingUser.Id,
      FirstName = request.FirstName,
      LastName = request.LastName,
      PhoneNumber = request.PhoneNumber,
      Address = request.Address,
    };

    var updateUserResult = await _identityService.UpdateUserDetails(user);

    if (updateUserResult)
    {
      return ApplicationResult<long>.SuccessResult(user.Id);
    }
    return ApplicationResult<long>.Error("Error occurred while updating User");
  }
}
