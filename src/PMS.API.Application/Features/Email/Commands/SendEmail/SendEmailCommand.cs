using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common;
using PMS.API.Application.Common.Helpers;
using PMS.API.Application.Common.Models;

namespace PMS.API.Application.Features.Email.Commands.SendEmail;

public class SendEmailCommand : IRequest<ApplicationResult<bool>>
{
  [Required]
  public required string ToEmail { get; set; }

  [Required]
  public required string Subject { get; set; }

  [Required]
  public required string Message { get; set; }
}

public class SendEmailCommandHandler : RequestHandlerBase<SendEmailCommand, ApplicationResult<bool>>
{
  private readonly IEmailSenderService _emailService;

  public SendEmailCommandHandler(
    IEmailSenderService emailService,
    IServiceProvider serviceProvider,
    ILogger<SendEmailCommandHandler> logger) : base(serviceProvider, logger)
  {
    _emailService = emailService;
  }

  protected override async Task<ApplicationResult<bool>> HandleRequest(SendEmailCommand request, CancellationToken cancellationToken)
  {
    var result = await _emailService.SendEmail(request.ToEmail, request.Subject, request.Message);
    
    if (result)
    {
      return ApplicationResult<bool>.SuccessResult(true, "Email sent successfully");
    }
    
    return ApplicationResult<bool>.Error("Failed to send email");
  }
}

