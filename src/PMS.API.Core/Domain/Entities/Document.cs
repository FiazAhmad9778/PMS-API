using PMS.API.Core.Domain.Interfaces;
using PMS.API.Core.Enums;
using PMS.API.SharedKernel.Interfaces;

namespace PMS.API.Core.Domain.Entities;
public class Document : IAggregateRoot, ISoftDeleteEntity
{
  public long Id { get; set; }
  public required string DocumentName { get; set; }
  public required string DocumentUrl { get; set; }
  public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
  public long? ModifiedBy { get; set; }
  public long? CreatedBy { get; set; }
  public DateTime? ModifiedDate { get; set; }
  public DocumentStatus Status { get; set; }
  public ICollection<DocumentMetadata> Metadata { get; set; } = new List<DocumentMetadata>();
  public bool IsDeleted { get; set; }
  public int NoOfPatients { get; set; }
}
