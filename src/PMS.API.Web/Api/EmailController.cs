using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.API.Application.Common.Models;
using PMS.API.Application.Features.Email.Commands.SendEmail;

namespace PMS.API.Web.Api;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class EmailController : BaseApiController
{
  public EmailController(IServiceProvider serviceProvider) : base(serviceProvider)
  {
  }

  [HttpPost("send")]
  [ProducesResponseType(typeof(ApplicationResult<bool>), StatusCodes.Status200OK)]
  public async Task<ApplicationResult<bool>> SendEmail(SendEmailCommand command)
  {
    return await Mediator.Send(command);
  }
}

