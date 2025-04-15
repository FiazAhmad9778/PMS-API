using PMS.API.SharedKernel.Interfaces;

namespace PMS.API.Core.Domain.Entities;
public class PMSErrorLog : BaseEntityWithState,  IAggregateRoot
{
  public string? Message { get; set; }
  public long ClientGroupId { get; set; }
  public long ClientId { get; set; }
  public long? TenantId { get; set; }
}
