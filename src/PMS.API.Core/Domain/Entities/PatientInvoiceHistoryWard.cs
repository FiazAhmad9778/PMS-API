using System.ComponentModel.DataAnnotations;

namespace PMS.API.Core.Domain.Entities;
public class PatientInvoiceHistoryWard
{
  [Key]
  public long Id { get; set; }
  public long PatientInvoiceHistoryId { get; set; }
  public long WardId { get; set; }
  public string? PatientIds { get; set; }
  public PatientInvoiceHistory? PatientInvoiceHistory { get; set; }
}
