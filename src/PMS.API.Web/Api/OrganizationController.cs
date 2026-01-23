using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Organizations.Commands.CreateOrganization;
using PMS.API.Application.Features.Organizations.DTO;
using PMS.API.Application.Features.Organizations.Queries.GetOrganizationDropdown;
using PMS.API.Application.Features.Organizations.Queries.GetOrganizations;
using PMS.API.Application.Features.Organizations.Queries.GetWardDropdown;
using PMS.API.Application.Features.Organizations.Queries.GetWards;

namespace PMS.API.Web.Api;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class OrganizationController : BaseApiController
{
  public OrganizationController(IServiceProvider serviceProvider) : base(serviceProvider)
  {
  }

  [HttpPost("save")]
  [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<long>> Save(CreateOrganizationCommand request)
  {
    return await Mediator.Send(request);
  }

  [HttpGet("list")]
  [ProducesResponseType(typeof(ApplicationResult<List<OrganizationResponseDto>>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<List<OrganizationResponseDto>>> GetOrganizations([FromQuery] GetOrganizationsQuery query)
  {
    return await Mediator.Send(query);
  }

  [HttpGet("wards")]
  [ProducesResponseType(typeof(ApplicationResult<List<WardResponseDto>>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<List<WardResponseDto>>> GetWards([FromQuery] GetWardsQuery query)
  {
    return await Mediator.Send(query);
  }

  [HttpGet("dropdown")]
  [ProducesResponseType(typeof(ApplicationResult<List<OrganizationResponseDto>>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<List<OrganizationResponseDto>>> GetOrganizationDropdown([FromQuery] GetOrganizationDropdownQuery query)
  {
    return await Mediator.Send(query);
  }

  [HttpGet("wards/dropdown")]
  [ProducesResponseType(typeof(ApplicationResult<List<WardResponseDto>>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<List<WardResponseDto>>> GetWardDropdown([FromQuery] GetWardDropdownQuery query)
  {
    return await Mediator.Send(query);
  }
}

