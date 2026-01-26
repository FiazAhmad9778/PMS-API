using System.ComponentModel.DataAnnotations;
using PMS.API.Core.Domain.Interfaces;
using PMS.API.SharedKernel.Interfaces;

namespace PMS.API.Core.Domain.Entities;
public class PatientInvoiceHistory : IAggregateRoot, ISoftDeleteEntity, IAuditEntity
{
  [Key]
  public long PatientId { get; set; }
  public DateTime InvoiceStartDate { get; set; }
  public DateTime InvoiceEndDate { get; set; }
  public string? FilePath { get; set; }

  public bool IsDeleted { get; set; }
  public DateTime CreatedDate { get; set; }
  public long? CreatedBy { get; set; }
  public DateTime? ModifiedDate { get; set; }
  public long? ModifiedBy { get; set; }
}
