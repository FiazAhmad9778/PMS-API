using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Users.DTO;
using PMS.API.Application.Services.Interfaces;
using PMS.API.Core.Domain.Entities.Identity;
using PMS.API.Core.Domain.Interfaces.Repositories;
using PMS.API.Core.DTOs.Base;
using PMS.API.Core.Enums;
using PMS.API.Core.Extensions;

namespace PMS.API.Application.Features.Users.Queries.GetUsers;

public class GetUsersQuery : PagedQueryBaseRequest, IRequest<ApplicationResult<List<UserResponseDto>>>
{
}

public class GetUsersQueryHandler : RequestHandlerBase<GetUsersQuery, ApplicationResult<List<UserResponseDto>>>
{
  private readonly IUserRepository _repository;
  private readonly IIdentityService _identityService;

  public GetUsersQueryHandler(IUserRepository repository,
    IServiceProvider serviceProvider,
    IIdentityService identityService,
    ILogger<GetUsersQueryHandler> logger) : base(serviceProvider, logger)
  {
    _repository = repository;
    _identityService = identityService;
  }

  protected override async Task<ApplicationResult<List<UserResponseDto>>> HandleRequest(GetUsersQuery request, CancellationToken cancellationToken)
  {
    request.SearchKeyword = !string.IsNullOrEmpty(request.SearchKeyword) ? request.SearchKeyword!.ToLower() : "";
    var users = _repository.Get().Where(x => x.UserType == AppUserTypeEnums.Technician);

    users = users.Where(p => string.IsNullOrEmpty(request.SearchKeyword)
      || p.Email!.ToLower().Contains(request.SearchKeyword!)
      || p.FirstName!.ToLower().Contains(request.SearchKeyword!)
      || p.LastName!.ToLower().Contains(request.SearchKeyword!)
      || p.PhoneNumber!.ToLower().Contains(request.SearchKeyword!));

    int totalCount = await users.CountAsync();
    List<User> userList;
    if (request.SortByAscending)
    {
      userList = await users.OrderBy(x => x.CreatedDate).Paginate(request.PageNumber, request.PageSize).ToListAsync();
    }
    else
    {
      userList = await users.OrderByDescending(x => x.CreatedDate).Paginate(request.PageNumber, request.PageSize).ToListAsync();
    }

    var result = new List<UserResponseDto>();

    foreach (var user in userList)
    {
      var roles = await _identityService.GetUserRolesAsync(user);

      var userDTO = _mapper.Map<UserResponseDto>(user);

      if (roles != null && roles.Count > 0)
      {
        userDTO.RoleId = roles[0].Id;
        userDTO.RoleName = roles[0].Name;
      }
      result.Add(userDTO);
    }
    return ApplicationResult<List<UserResponseDto>>.SuccessResult(result, totalCount);
  }
}
