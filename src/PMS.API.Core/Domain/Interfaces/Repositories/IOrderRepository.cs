using PMS.API.Core.Domain.Entities;

namespace PMS.API.Core.Domain.Interfaces.Repositories;

public interface IOrderRepository : IEfRepository<Order>
{
  Task<Order?> GetByWebhookIdAsync(string webhookId);
}


