using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PMS.API.Application.Common.Context.Models;
using PMS.API.Application.Common.Enums;
using PMS.API.Application.DTOs.Common.Base.Response;
using PMS.API.Core.Domain.Entities;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Application.Common.Helpers;

public class ExceptionHelper : IExceptionHelper
{

  public ExceptionHelper(
    IEmailSenderService emailHandler,
    AppDbContext dbContext,
    ILogger<ExceptionHelper> logger,
    IConfiguration configuration/* IHostingEnvironment hostingEnvironment*/
    )
  {
    _configuration = configuration;
    _emailHandler = emailHandler;
    _dbContext = dbContext;
    _logger = logger;
    //_hostingEnvironment = hostingEnvironment;

  }


  //private readonly IHostingEnvironment _hostingEnvironment;
  private readonly IEmailSenderService _emailHandler;
  private readonly AppDbContext _dbContext;
  private readonly ILogger<ExceptionHelper> _logger;

  private IConfiguration _configuration { get; }

  public async Task<ErrorResponse> GetErrorResponse(Exception ex, CurrentUserContext currentUser, bool sendEmail = false, string? customMessage = null!, bool logToDb = true)
  {
    var errorId = (long)PMSErrorEnums.SomethingWentWrong;

    if (logToDb)
    {
      errorId = await Log(ex, currentUser, sendEmail: sendEmail);
    }

    string errorMessage;
    //if no custom message then add the error log //  in case of Exception type
    if (string.IsNullOrWhiteSpace(customMessage))
    {
      errorMessage = $"Something went wrong on the server, Please reach out to support and refer to this errorId: {errorId}";
    }
    else //else add the custom message
    {
      errorMessage = customMessage;
    }

    return new ErrorResponse { Errors = new List<string> { errorMessage }, IsSuccess = false, ErrorId = PMSErrorEnums.SomethingWentWrong };
  }

  //[Obsolete]
  public async Task<long> Log(Exception e, CurrentUserContext currentUser, bool sendEmail = false)
  {
    var Description = string.Empty;

    if (!string.IsNullOrEmpty(Convert.ToString(e.InnerException)))
    {
      Description = " InnerException: " + e.InnerException;
    }
    if (!string.IsNullOrEmpty(Convert.ToString(e.Message)))
    {
      if (!string.IsNullOrEmpty(Description))
      {
        Description += Environment.NewLine;
      }
      Description += " Message: " + e.Message;
    }
    if (!string.IsNullOrEmpty(Convert.ToString(e.StackTrace)))
    {
      if (!string.IsNullOrEmpty(Description))
      {
        Description += Environment.NewLine;
      }
      Description += " StackTrace: " + e.StackTrace;
    }
    _logger.LogError(Description);
    PMSErrorLog log = new PMSErrorLog
    {
      Message = Description,
    };


    _dbContext.PMSErrorLogs.Add(log);
    await _dbContext.SaveChangesAsync();

    //if (!string.IsNullOrEmpty(Convert.ToString(_hostingEnvironment.EnvironmentName)))
    //{
    //  if (!string.IsNullOrEmpty(Description))
    //  {
    //    Description += Environment.NewLine;
    //  }
    //  Description += " Environment: " + _hostingEnvironment.EnvironmentName;
    //}

    if (sendEmail && _configuration.GetValue<bool>("EmailConfiguration:SendMail"))
    {
      //var emailSubject = "Error occurred on the project";
      //var emailContent = $"Dear Admin ,<br/><br/>{Description}<br/><br/>Regards <br/>Support Team";
      ////Sending Email
      //await _emailHandler.SendEmailAsync("z.adil@PMS.com", emailSubject, emailContent, "");
    }
    return log.Id;
  }
}
