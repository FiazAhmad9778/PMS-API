using Microsoft.EntityFrameworkCore;
using PMS.API.Core.Domain.Entities;
using PMS.API.Core.Domain.Interfaces.Repositories;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Infrastructure.Repositories;

public class OrderRepository : EfRepository<Order>, IOrderRepository
{
  private readonly AppDbContext _context;
  public OrderRepository(AppDbContext context) : base(context)
  {
    _context = context;
  }

  public async Task<Order?> GetByWebhookIdAsync(string webhookId)
  {
    return await _context.Order
      .FirstOrDefaultAsync(o => o.WebhookId == webhookId);
  }
}


