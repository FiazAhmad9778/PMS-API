using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Invoice.Command;
using PMS.API.Application.Features.Invoice.DTO;
using PMS.API.Application.Features.Invoices.Commands.GenerateInvoice;
using PMS.API.Application.Features.Invoices.Commands.RequestInvoice;
using PMS.API.Application.Features.Invoices.DTO;
using PMS.API.Application.Features.Invoices.Queries.GetInvoiceHistoryList;
using PMS.API.Application.Features.Invoices.Queries.GetInvoiceDownloadPath;

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
}

