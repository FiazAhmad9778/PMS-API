namespace PMS.API.Application.Features.Patients.DTO;

public class PatientFinancialResponseDto
{
  public long PatientId { get; set; }
  public DateTime Date { get; set; }
  public string PatientName { get; set; } = string.Empty;
  public string? YourCode { get; set; }
  public long WardId { get; set; }
  public string? SeamCode { get; set; }
  public string? ChargeDescription { get; set; }
  public string TaxType { get; set; } = string.Empty;
  public decimal Amount { get; set; }
}
