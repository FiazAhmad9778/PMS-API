using System.ComponentModel.DataAnnotations.Schema;
using PMS.API.Core.Domain.Interfaces;
using PMS.API.SharedKernel.Interfaces;

namespace PMS.API.Core.Domain.Entities;

[Table("Patient")]
public class Patient : IAggregateRoot, ISoftDeleteEntity, IAuditEntity
{
  public long Id { get; set; }
  public required long PatientId { get; set; }
  public required string Name { get; set; }
  public DateTime CreatedDate { get; set; }
  public string? DefaultEmail { get; set; }
  public string? Address { get; set; }
  public string Status { get; set; } = "active";
  public long? CreatedBy { get; set; }
  public DateTime? ModifiedDate { get; set; }
  public long? ModifiedBy { get; set; }
  public bool IsDeleted { get; set; }

  public List<InvoiceHistory> InvoiceHistoryList { get; set; } = new List<InvoiceHistory>();
}

