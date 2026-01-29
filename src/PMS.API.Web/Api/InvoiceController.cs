using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Invoice.Command;
using PMS.API.Application.Features.Invoice.DTO;
using PMS.API.Application.Features.Invoices.Commands.GenerateInvoice;
using PMS.API.Application.Features.Patients.Commands.ExportOrganizationCharges;

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

  [HttpPost("generate")]
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

  [HttpPost("organizationCharges/excel")]
  [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
  public async Task<IActionResult> OrganizationChargesExcel(
      [FromBody] ExportOrganizationChargesCommand command)
  {
    var result = await Mediator.Send(command);

    if (!result.Success || result.Data == null)
      return BadRequest(result);

    var webRootPath = _env.WebRootPath;

    var excelBytes =
        ExcelExportHelper.GenerateOrganizationChargesExcel(
          result.Data.Charges ?? new(),
          result.Data.Clients ?? new(),
          command.FromDate,
          command.ToDate,
          webRootPath);

    return File(
        excelBytes,
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        $"OrganizationCharges_{command.NHID}_{DateTime.UtcNow:yyyyMMdd}.xlsx"
    );
  }
}

