using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Invoices.Commands.GenerateInvoice;

namespace PMS.API.Web.Api;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class InvoiceController : BaseApiController
{
  public InvoiceController(IServiceProvider serviceProvider) : base(serviceProvider)
  {
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
}

