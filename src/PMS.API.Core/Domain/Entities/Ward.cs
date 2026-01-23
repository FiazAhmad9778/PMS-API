using System.ComponentModel.DataAnnotations.Schema;
using PMS.API.Core.Domain.Interfaces;
using PMS.API.SharedKernel.Interfaces;

namespace PMS.API.Core.Domain.Entities;

[Table("Ward")]
public class Ward : IAggregateRoot, ISoftDeleteEntity, IAuditEntity
{
  public long Id { get; set; }
  public required string Name { get; set; }
  public string? ExternalId { get; set; }
  public long? OrganizationId { get; set; }
  public Organization? Organization { get; set; }
  public DateTime CreatedDate { get; set; }
  public long? CreatedBy { get; set; }
  public DateTime? ModifiedDate { get; set; }
  public long? ModifiedBy { get; set; }
  public bool IsDeleted { get; set; }
}
