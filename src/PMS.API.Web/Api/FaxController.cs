using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Fax.Commands.SendFax;

namespace PMS.API.Web.Api;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class FaxController : BaseApiController
{
  public FaxController(IServiceProvider serviceProvider) : base(serviceProvider)
  {
  }

  [HttpPost("send")]
  [ProducesResponseType(typeof(ApplicationResult<bool>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<bool>> SendFax([FromForm] SendFaxCommand command)
  {
    return await Mediator.Send(command);
  }
}

