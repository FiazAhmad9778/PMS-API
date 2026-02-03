using PMS.API.Application.Features.Patients.DTO;

namespace PMS.API.Application.Services.Interfaces;

public interface IInvoiceFileGenerationService
{
  /// <summary>
  /// Generates the invoice Excel file for an organization. Returns (filePath, charges) or (null, null) if no charges.
  /// </summary>
  Task<(string? FilePath, List<PatientFinancialResponseDto>? Charges)> GenerateOrganizationInvoiceAsync(
    long organizationId,
    DateTime fromDate,
    DateTime toDate,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Generates the invoice Excel file for a patient. Returns file path or null if no charges.
  /// </summary>
  Task<string?> GeneratePatientInvoiceAsync(
    long patientId,
    DateTime fromDate,
    DateTime toDate,
    CancellationToken cancellationToken = default);
}
