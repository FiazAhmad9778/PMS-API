namespace PMS.API.Application.Features.Organizations.DTO;

public class OrganizationResponseDto
{
  public long Id { get; set; }
  public long OrganizationExternalId { get; set; }
  public string? Name { get; set; }
  public long[]? WardIds { get; set; }
  public List<WardResponseDto> Wards { get; set; } = new List<WardResponseDto>();
  public string? Address { get; set; }
  public string? DefaultEmail { get; set; }
  public DateTime CreatedDate { get; set; }
  public DateTime? ModifiedDate { get; set; }
  public string? InvoicePath { get; set; }
  /// <summary>True if the latest invoice for this org has been sent. Enables resend icon in UI.</summary>
  public bool? InvoiceIsSent { get; set; }
  /// <summary>Pending invoice period start (when used in get-pending-invoices).</summary>
  public DateTime? InvoiceFromDate { get; set; }
  /// <summary>Pending invoice period end (when used in get-pending-invoices).</summary>
  public DateTime? InvoiceToDate { get; set; }
}

