using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PMS.API.Application.Common.BackgroundWorker;
public class QueuedHostedService : BackgroundService
{
  private readonly IBackgroundTaskQueue _taskQueue;
  private readonly IServiceScopeFactory _scopeFactory;

  public QueuedHostedService(
    IBackgroundTaskQueue taskQueue,
    IServiceScopeFactory scopeFactory)
  {
    _taskQueue = taskQueue;
    _scopeFactory = scopeFactory;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      var workItem = await _taskQueue.DequeueAsync(stoppingToken);

      try
      {
        using var scope = _scopeFactory.CreateScope();
        await workItem(stoppingToken);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
      }
    }
  }
}

