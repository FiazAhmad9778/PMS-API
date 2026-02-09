using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Invoice.Command;
using PMS.API.Application.Features.Invoice.DTO;
using PMS.API.Application.Features.Invoices.Commands.GenerateAndSaveInvoice;
using PMS.API.Application.Features.Invoices.Commands.GenerateInvoice;
using PMS.API.Application.Features.Invoices.Commands.RequestInvoice;
using PMS.API.Application.Features.Invoices.DTO;
using PMS.API.Application.Features.Invoices.Queries.GetInvoiceHistoryList;
using PMS.API.Application.Features.Invoices.Queries.GetInvoiceDownloadPath;
using PMS.API.Application.Features.Invoices.Queries.GetPendingInvoicesOrganization;
using PMS.API.Application.Features.Invoices.Queries.GetPendingInvoicesPatient;
using PMS.API.Application.Features.Invoices.Commands.SendInvoicesEmail;
using PMS.API.Application.Features.Invoices.Commands.ResendInvoicesEmail;
using PMS.API.Application.Features.Organizations.DTO;
using PMS.API.Application.Features.Patients.DTO;

namespace PMS.API.Web.Api;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class InvoiceController : BaseApiController
{
  readonly IWebHostEnvironment _env;
  public InvoiceController(IServiceProvider serviceProvider, IWebHostEnvironment env) : base(serviceProvider)
  {
    _env = env;
  }

  [HttpPost("export")]
  [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
  public async Task<IActionResult> GenerateInvoice([FromBody] GenerateInvoiceCommand command)
  {
    var result = await Mediator.Send(command);

    if (result.IsSuccess && result.Data != null)
    {
      var fileName = $"Invoice_{command.PatientId}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
      return new FileContentResult(result.Data, "application/pdf")
      {
        FileDownloadName = fileName
      };
    }

    return BadRequest(result);
  }

  [HttpPost("chargesInvoice")]
  [ProducesResponseType(typeof(ApplicationResult<List<ChargesReportDto>>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<List<ChargesReportDto>>> ChargesInvoice(
     ChargesInvoiceCommand command)
  {
    return await Mediator.Send(command);
  }

  [HttpPost("chargesInvoice/excel")]
  public async Task<IActionResult> ChargesInvoiceExcel(
      ChargesInvoiceCommand command)
  {
    var result = await Mediator.Send(command);

    if (!result.Success || result.Data == null || !result.Data.Any())
      return BadRequest(result);

    var excelBytes =
        ExcelExportHelper.GenerateChargesExcel(result.Data);

    return File(
        excelBytes,
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        $"ChargesInvoice_{command.PatientId}_{DateTime.UtcNow:yyyyMMdd}.xlsx"
    );
  }

  [HttpPost("clientInvoice")]
  [ProducesResponseType(typeof(ApplicationResult<List<ClientReportDto>>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<List<ClientReportDto>>> ClientInvoice(
      ClientInvoiceCommand command)
  {
    return await Mediator.Send(command);
  }

  [HttpPost("clientInvoice/excel")]
  public async Task<IActionResult> ClientInvoiceExcel(
      ClientInvoiceCommand command)
  {
    var result = await Mediator.Send(command);

    if (!result.Success || result.Data == null || !result.Data.Any())
      return BadRequest(result);

    var excelBytes =
        ExcelExportHelper.GenerateClientExcel(result.Data);

    return File(
        excelBytes,
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        $"ClientInvoice_{command.PatientId}_{DateTime.UtcNow:yyyyMMdd}.xlsx"
    );
  }

  [HttpPost("generate-invoice")]
  public async Task<IActionResult> RequestInvoice(
    [FromBody] RequestInvoiceCommand command)
  {
    var result = await Mediator.Send(command);

    if (!result.Success)
      return BadRequest(result);

    return Ok(new { success = true });
  }

  /// <summary>
  /// Creates invoice history entries, immediately generates invoice files, and marks status as Completed.
  /// If the same period already has an unsent invoice, it is deleted and recreated. If already sent, that org/patient is skipped and their names are returned in the response.
  /// </summary>
  [HttpPost("generate-and-save")]
  [ProducesResponseType(typeof(ApplicationResult<GenerateAndSaveInvoiceResultDto>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GenerateAndSaveInvoice([FromBody] GenerateAndSaveInvoiceCommand command)
  {
    var result = await Mediator.Send(command);
    if (!result.Success)
      return BadRequest(result);
    return Ok(result);
  }

  /// <summary>
  /// Returns created invoices from invoice history. By default returns all; optionally filter by internal organization and/or patient IDs.
  /// </summary>
  /// <param name="organizationIds">Internal organization IDs (Organization.Id).</param>
  /// <param name="patientIds">Internal patient IDs (Patient.Id).</param>
  [HttpGet("history")]
  [ProducesResponseType(typeof(ApplicationResult<List<InvoiceHistoryItemDto>>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetInvoiceHistory(
    [FromQuery] List<long>? organizationIds = null,
    [FromQuery] List<long>? patientIds = null)
  {
    var query = new GetInvoiceHistoryListQuery
    {
      OrganizationIds = organizationIds,
      PatientIds = patientIds
    };
    var result = await Mediator.Send(query);
    if (!result.Success)
      return BadRequest(result);
    return Ok(result);
  }

  /// <summary>
  /// Downloads the invoice file for the given invoice history id. Use the DownloadUrl from the history list for the link.
  /// </summary>
  [HttpGet("download/{id:long}")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> DownloadInvoice(long id)
  {
    var pathResult = await Mediator.Send(new GetInvoiceDownloadPathQuery { InvoiceHistoryId = id });
    if (!pathResult.Success || string.IsNullOrWhiteSpace(pathResult.Data))
      return NotFound();

    var fullPath = Path.Combine(_env.WebRootPath ?? "wwwroot", pathResult.Data!);
    if (!System.IO.File.Exists(fullPath))
      return NotFound();

    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    var fileName = Path.GetFileName(pathResult.Data);
    return PhysicalFile(fullPath, contentType, fileName);
  }

  /// <summary>
  /// Returns organizations that have a latest invoice generated but not sent. For use on the organization tab "Send invoices" flow.
  /// </summary>
  [HttpGet("organization/get-pending-invoices")]
  [ProducesResponseType(typeof(ApplicationResult<List<OrganizationResponseDto>>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetPendingInvoicesOrganization(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 50,
    [FromQuery] string? searchKeyword = null)
  {
    var result = await Mediator.Send(new GetPendingInvoicesOrganizationQuery
    {
      PageNumber = pageNumber,
      PageSize = pageSize,
      SearchKeyword = searchKeyword
    });
    if (!result.Success)
      return BadRequest(result);
    return Ok(result);
  }

  /// <summary>
  /// Returns patients that have a latest invoice generated but not sent. For use on the patient tab "Send invoices" flow.
  /// </summary>
  [HttpGet("patient/get-pending-invoices")]
  [ProducesResponseType(typeof(ApplicationResult<List<PatientResponseDto>>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetPendingInvoicesPatient(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 50,
    [FromQuery] string? searchKeyword = null)
  {
    var result = await Mediator.Send(new GetPendingInvoicesPatientQuery
    {
      PageNumber = pageNumber,
      PageSize = pageSize,
      SearchKeyword = searchKeyword
    });
    if (!result.Success)
      return BadRequest(result);
    return Ok(result);
  }

  /// <summary>
  /// Marks invoices as sent for the given organization and/or patient IDs. Same API for both organization and patient tabs.
  /// </summary>
  [HttpPost("send-invoices-email")]
  [ProducesResponseType(typeof(ApplicationResult<bool>), StatusCodes.Status200OK)]
  public async Task<IActionResult> SendInvoicesEmail([FromBody] SendInvoicesEmailCommand command)
  {
    command.WebRootPath = _env.WebRootPath ?? "wwwroot";
    var result = await Mediator.Send(command);
    if (!result.Success)
      return BadRequest(result);
    return Ok(result);
  }

  /// <summary>
  /// Resends the invoice email for a single organization or patient. Body: { "organizationId": 1 } or { "patientId": 5 }. Marks invoice as sent (IsSent) after successful send so UI can use this for both send and resend.
  /// </summary>
  [HttpPost("resend-invoices-email")]
  [ProducesResponseType(typeof(ApplicationResult<bool>), StatusCodes.Status200OK)]
  public async Task<IActionResult> ResendInvoicesEmail([FromBody] ResendInvoicesEmailCommand command)
  {
    command.WebRootPath = _env.WebRootPath ?? "wwwroot";
    var result = await Mediator.Send(command);
    if (!result.Success)
      return BadRequest(result);
    return Ok(result);
  }
}

