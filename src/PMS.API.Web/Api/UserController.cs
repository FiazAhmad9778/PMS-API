using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Users.Commands.CreateUser;
using PMS.API.Application.Features.Users.Commands.DeleteUser;
using PMS.API.Application.Features.Users.Commands.UpdateUser;
using PMS.API.Application.Features.Users.Commands.UpdateUserProfile;
using PMS.API.Application.Features.Users.DTO;
using PMS.API.Application.Features.Users.Queries.GetUser;
using PMS.API.Application.Features.Users.Queries.GetUserProfile;
using PMS.API.Application.Features.Users.Queries.GetUsers;

namespace PMS.API.Web.Api;
[Route("api/[controller]")]
[Authorize]
[ApiController]
public class UserController : BaseApiController
{
  public UserController(IServiceProvider serviceProvider) : base(serviceProvider)
  {

  }
  [HttpPost("save")]
  [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<long>> Save(CreateUserCommand request)
  {
    return await Mediator.Send(request);
  }

  [HttpPut]
  [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<long>> UpdateUser(UpdateUserCommand command)
  {
    return await Mediator.Send(command);
  }

  [HttpPut("update-profile")]
  [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<long>> UpdateUserProfile([FromForm] UpdateUserProfileCommand command)
  {
    return await Mediator.Send(command);
  }

  [HttpPut("update-signature")]
  [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<long>> UpdateUserSignature([FromForm] UpdateUserSignatureCommand command)
  {
    return await Mediator.Send(command);
  }

  [HttpGet("get")]
  [ProducesResponseType(typeof(ApplicationResult<UserResponseDto>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<UserResponseDto>> GetUser([FromQuery] GetUserQuery query)
  {
    return await Mediator.Send(query);
  }

  [HttpGet("get-profile")]
  [ProducesResponseType(typeof(ApplicationResult<UserResponseDto>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<UserResponseDto>> GetUserProfile([FromQuery] GetUserProfileQuery query)
  {
    return await Mediator.Send(query);
  }

  [HttpGet("list")]
  [ProducesResponseType(typeof(ApplicationResult<List<UserResponseDto>>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<List<UserResponseDto>>> GetUsers([FromQuery] GetUsersQuery query)
  {
    return await Mediator.Send(query);
  }

  [HttpDelete("delete")]
  [ProducesResponseType(typeof(ApplicationResult<bool>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<bool>> DeleteUser([FromQuery] DeleteUserCommand command)
  {
    return await Mediator.Send(command);
  }
}
