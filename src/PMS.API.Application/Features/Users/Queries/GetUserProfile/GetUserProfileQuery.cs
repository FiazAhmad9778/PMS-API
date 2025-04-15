using MediatR;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Users.DTO;
using PMS.API.Application.Services.Interfaces;
using PMS.API.Core.Domain.Interfaces.Repositories;

namespace PMS.API.Application.Features.Users.Queries.GetUserProfile;
public class GetUserProfileQuery : IRequest<ApplicationResult<UserResponseDto>>
{
}

public class GetUserProfileQueryHandler : RequestHandlerBase<GetUserProfileQuery, ApplicationResult<UserResponseDto>>
{

  private readonly IUserRepository _repository;
  private readonly IIdentityService _identityService;

  public GetUserProfileQueryHandler(IUserRepository repository,
    IIdentityService identityService, IServiceProvider serviceProvider, ILogger<GetUserProfileQueryHandler> logger) : base(serviceProvider, logger)
  {
    _repository = repository;
    _identityService = identityService;
  }

  protected override async Task<ApplicationResult<UserResponseDto>> HandleRequest(GetUserProfileQuery request, CancellationToken cancellationToken)
  {

    var user = await _repository
                      .FirstAsync(x => x.Id == _currentUser.Id);

    if (user == null) return ApplicationResult<UserResponseDto>.Error("User not found!");
    var roles = await _identityService.GetUserRolesAsync(user);
    var result = _mapper.Map<UserResponseDto>(user);
    if (roles != null && roles.Count > 0)
    {
      result.RoleId = roles[0].Id;
      result.RoleName = roles[0].Name;
    }
    return ApplicationResult<UserResponseDto>.SuccessResult(result);

  }
}
