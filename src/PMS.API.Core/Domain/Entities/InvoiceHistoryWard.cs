using System.ComponentModel.DataAnnotations;

namespace PMS.API.Core.Domain.Entities;

public class InvoiceHistoryWard
{
  [Key]
  public long Id { get; set; }
  public long InvoiceHistoryId { get; set; }
  public long WardId { get; set; }
  public string? PatientIds { get; set; }
  public InvoiceHistory? InvoiceHistory { get; set; }
}
