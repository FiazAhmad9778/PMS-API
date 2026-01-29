namespace PMS.API.Application.Common.BackgroundWorker;
public interface IBackgroundTaskQueue
{
  void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);

  Task<Func<CancellationToken, Task>> DequeueAsync(
    CancellationToken cancellationToken);
}
