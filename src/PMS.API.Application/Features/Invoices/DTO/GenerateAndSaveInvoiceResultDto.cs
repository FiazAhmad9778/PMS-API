namespace PMS.API.Application.Features.Invoices.DTO;

/// <summary>
/// Result of generate-and-save invoice API. Lists organizations/patients skipped because an invoice for the period was already sent.
/// </summary>
public class GenerateAndSaveInvoiceResultDto
{
  /// <summary>Organization names that were skipped (invoice for same period already sent).</summary>
  public List<string> SkippedOrganizationNames { get; set; } = new();

  /// <summary>Patient names that were skipped (invoice for same period already sent).</summary>
  public List<string> SkippedPatientNames { get; set; } = new();
}
