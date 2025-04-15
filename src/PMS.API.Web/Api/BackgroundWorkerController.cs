using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.API.Application.Common.Models;

namespace PMS.API.Web.Api;
[Route("api/[controller]")]
[Authorize]
[ApiController]
public class BackgroundWorkerController : BaseApiController
{
  public BackgroundWorkerController(IServiceProvider serviceProvider) : base(serviceProvider)
  {
  }

  [HttpGet("trigger")]
  public IActionResult TriggerWorker()
  {
    using (var connection = JobStorage.Current.GetConnection())
    {
      var processingJobs = connection.GetRecurringJobs()
          .FirstOrDefault(job => job.Id == "sync-documents" && job.LastJobState == "Processing");

      if (processingJobs != null)
      {
        Console.WriteLine("⏳ Job is already running. Ignoring request.");
        return Conflict("Job is already running. Please wait for it to complete.");
      }
    }

    RecurringJob.TriggerJob("sync-documents");

    return Ok(ApplicationResult<bool>.SuccessResult(true));
  }

  [HttpGet("status")]
  public IActionResult GetWorkerStatus()
  {
    using (var connection = JobStorage.Current.GetConnection())
    {
      var processingJobs = connection.GetRecurringJobs()
          .FirstOrDefault(job => job.Id == "sync-documents" && job.LastJobState == "Processing");

      if (processingJobs != null)
      {
        Console.WriteLine("⏳ Job is already running. Ignoring request.");
        return Ok(ApplicationResult<string>.SuccessResult("running"));
      }
    }


    return Ok(ApplicationResult<string>.SuccessResult("completed"));
  }
}
