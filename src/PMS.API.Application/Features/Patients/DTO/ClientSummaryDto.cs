namespace PMS.API.Application.Features.Patients.DTO;

public class ClientSummaryDto
{
  public long PatientId { get; set; }
  public string? PatientName { get; set; }
  public long WardId { get; set; }
  public string? LocationHome { get; set; }
  public string? SeamLessCode { get; set; }
  public decimal ChargesOnAccount { get; set; }
  public decimal TaxIncluded { get; set; }
  public decimal PaymentsMade { get; set; }
  public decimal OutstandingCharges { get; set; }
}
