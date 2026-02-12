namespace PMS.API.Application.Features.Patients.DTO;

public class PatientResponseDto
{
  public long Id { get; set; }
  public long? PatientId { get; set; } // External ID from Kroll database (ARID)
  public required string Name { get; set; }
  public string? Address { get; set; }
  public string? DefaultEmail { get; set; }
  public string Status { get; set; } = "active";
  public DateTime CreatedDate { get; set; }
  /// <summary>Id of the latest invoice in InvoiceHistory. Use with api/Invoice/download/{id} to download.</summary>
  public long? LastInvoiceId { get; set; }
  /// <summary>True if the latest invoice for this patient has been sent. Enables resend icon in UI.</summary>
  public bool? InvoiceIsSent { get; set; }
  /// <summary>Pending invoice period start (when used in get-pending-invoices).</summary>
  public DateTime? InvoiceFromDate { get; set; }
  /// <summary>Pending invoice period end (when used in get-pending-invoices).</summary>
  public DateTime? InvoiceToDate { get; set; }
}

