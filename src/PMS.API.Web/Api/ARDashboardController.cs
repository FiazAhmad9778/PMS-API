using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.ARDashboard.Queries.GetARDashboardOrganizations;
using PMS.API.Application.Features.ARDashboard.Queries.GetARDashboardPatients;
using PMS.API.Application.Features.Organizations.DTO;
using PMS.API.Application.Features.Patients.DTO;
using PMS.API.Application.Features.Patients.Queries.GetPatientFinancials;

namespace PMS.API.Web.Api;

[Route("api/ar-dashboard")]
[Authorize]
[ApiController]
public class ARDashboardController : BaseApiController
{
  public ARDashboardController(IServiceProvider serviceProvider) : base(serviceProvider)
  {
  }

  [HttpGet("patient/list")]
  [ProducesResponseType(typeof(ApplicationResult<List<PatientResponseDto>>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<List<PatientResponseDto>>> GetPatients([FromQuery] GetARDashboardPatientsQuery query)
  {
    return await Mediator.Send(query);
  }

  [HttpGet("organization/list")]
  [ProducesResponseType(typeof(ApplicationResult<List<OrganizationResponseDto>>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<List<OrganizationResponseDto>>> GetOrganizations([FromQuery] GetARDashboardOrganizationsQuery query)
  {
    return await Mediator.Send(query);
  }

  [HttpGet("patient/financials")]
  [ProducesResponseType(typeof(ApplicationResult<List<PatientFinancialResponseDto>>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<List<PatientFinancialResponseDto>>> GetPatientFinancials([FromQuery] GetPatientFinancialsQuery query)
  {
    return await Mediator.Send(query);
  }
}

