using AutoMapper;
using MediatR;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Users.DTO;
using PMS.API.Application.Services.Interfaces;
using PMS.API.Core.Domain.Interfaces.Repositories;

namespace PMS.API.Application.Features.Users.Queries.GetUser;

public class GetUserQuery : IRequest<ApplicationResult<UserResponseDto>>
{
  public long Id { get; set; }
}

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, ApplicationResult<UserResponseDto>>
{

  private readonly IMapper _mapper;
  private readonly IUserRepository _repository;
  private readonly IIdentityService _identityService;

  public GetUserQueryHandler(IUserRepository repository,
    IIdentityService identityService,
    IMapper mapper)
  {
    _repository = repository;
    _identityService = identityService;
    _mapper = mapper;
  }

  public async Task<ApplicationResult<UserResponseDto>> Handle(GetUserQuery request, CancellationToken cancellationToken)
  {

    var user = await _repository
                      .FirstAsync(x => x.Id == request.Id);

    if (user == null) return ApplicationResult<UserResponseDto>.Error("User not found!");
    var roles = await _identityService.GetUserRolesAsync(user);

    var result = _mapper.Map<UserResponseDto>(user);
    if (roles != null && roles.Count > 0)
    {
      result.RoleId = roles[0].Id;
      result.RoleName = roles[0].Name;
    }

    if (user.SignatureData != null)
    {
      result.SignatureBase64 = Convert.ToBase64String(user.SignatureData);
    }

    return ApplicationResult<UserResponseDto>.SuccessResult(result);

  }
}
