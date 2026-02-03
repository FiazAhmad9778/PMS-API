using System.ComponentModel.DataAnnotations;
using PMS.API.Core.Domain.Interfaces;
using PMS.API.SharedKernel.Interfaces;

namespace PMS.API.Core.Domain.Entities;

public class InvoiceHistory : IAggregateRoot, ISoftDeleteEntity, IAuditEntity
{
  public InvoiceHistory()
  {
    InvoiceHistoryWardList = new List<InvoiceHistoryWard>();
  }

  [Key]
  public long Id { get; set; }
  public long? OrganizationId { get; set; }
  public long? PatientId { get; set; }
  public bool IsSent { get; set; }
  public string? InvoiceStatus { get; set; }
  public string? InvoiceStatusHistory { get; set; }
  public DateTime InvoiceStartDate { get; set; }
  public DateTime InvoiceEndDate { get; set; }
  public string? FilePath { get; set; }
  public long? CreatedBy { get; set; }
  public DateTime CreatedDate { get; set; }
  public DateTime? ModifiedDate { get; set; }
  public long? ModifiedBy { get; set; }
  public bool IsDeleted { get; set; }

  public List<InvoiceHistoryWard> InvoiceHistoryWardList { get; set; }
}
