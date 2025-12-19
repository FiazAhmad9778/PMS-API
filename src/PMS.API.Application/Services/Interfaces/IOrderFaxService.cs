namespace PMS.API.Application.Services.Interfaces;

public interface IOrderFaxService
{
  Task ProcessPendingOrdersAsync(CancellationToken cancellationToken = default);
}
