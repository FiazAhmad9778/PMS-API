namespace PMS.API.Application.Features.Invoice.DTO;
public class ClientReportDto
{
  public string? PatientName { get; set; }
  public string? LocationHome { get; set; }
  public string? SeamLessCode { get; set; }

  public decimal ChargesOnAccount { get; set; }
  public decimal TaxIncluded { get; set; }
  public decimal PaymentsMade { get; set; }
  public decimal OutstandingCharges { get; set; }
}

