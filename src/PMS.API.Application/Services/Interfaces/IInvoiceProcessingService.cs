namespace PMS.API.Application.Services.Interfaces;

public interface IInvoiceProcessingService
{
  /// <summary>
  /// Processes all pending invoice records: generates Excel files and updates records with file path and status.
  /// </summary>
  Task ProcessPendingInvoicesAsync(CancellationToken cancellationToken = default);
}
