namespace PMS.API.Application.Features.Invoice.DTO;
public class ChargesReportDto
{
  public DateTime Date { get; set; }
  public string? PatientName { get; set; }
  public string? YourCode { get; set; }
  public string? SeamCode { get; set; }
  public string? ChargeDescription { get; set; }
  public string? TaxType { get; set; }
  public decimal Amount { get; set; }
}

