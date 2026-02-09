namespace PMS.API.Application.Features.Invoices.DTO;

public class InvoiceHistoryItemDto
{
  public long Id { get; set; }
  /// <summary>Invoice type for UI: "Organization" or "Patient".</summary>
  public string InvoiceType { get; set; } = string.Empty;
  /// <summary>Organization name for org invoices, or patient name for patient invoices.</summary>
  public string? Name { get; set; }
  public string? InvoiceStatus { get; set; }
  public string? FilePath { get; set; }
  /// <summary>Relative URL for download button, e.g. "api/Invoice/download/123".</summary>
  public string? DownloadUrl { get; set; }
  public DateTime InvoiceStartDate { get; set; }
  public DateTime InvoiceEndDate { get; set; }
  public DateTime CreatedDate { get; set; }
  public bool IsSent { get; set; }
}
