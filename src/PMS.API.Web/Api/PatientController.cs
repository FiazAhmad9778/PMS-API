using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Patients.Commands.AddPatientsByFilter;
using PMS.API.Application.Features.Patients.Commands.CreatePatient;
using PMS.API.Application.Features.Patients.DTO;
using PMS.API.Application.Features.Patients.Queries.GetPatientDropdown;
using PMS.API.Application.Features.Patients.Queries.GetPatients;

namespace PMS.API.Web.Api;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class PatientController : BaseApiController
{
  public PatientController(IServiceProvider serviceProvider) : base(serviceProvider)
  {
  }

  [HttpPost("save")]
  [ProducesResponseType(typeof(ApplicationResult<long>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<long>> Save(CreatePatientCommand request)
  {
    return await Mediator.Send(request);
  }

  [HttpGet("list")]
  [ProducesResponseType(typeof(ApplicationResult<List<PatientResponseDto>>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<List<PatientResponseDto>>> GetPatients([FromQuery] GetPatientsQuery query)
  {
    return await Mediator.Send(query);
  }

  [HttpPost("add-by-filter")]
  [ProducesResponseType(typeof(ApplicationResult<int>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<int>> AddPatientsByFilter(AddPatientsByFilterCommand request)
  {
    return await Mediator.Send(request);
  }

  [HttpGet("dropdown")]
  [ProducesResponseType(typeof(ApplicationResult<List<PatientResponseDto>>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<List<PatientResponseDto>>> GetPatientDropdown([FromQuery] GetPatientDropdownQuery query)
  {
    return await Mediator.Send(query);
  }
}

