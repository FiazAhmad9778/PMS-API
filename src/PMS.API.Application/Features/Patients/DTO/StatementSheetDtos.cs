namespace PMS.API.Application.Features.Patients.DTO;

public class StatementToDto
{
  public string Name { get; set; } = string.Empty;
  public string? Address { get; set; }
}

public class StatementFromDto
{
  public string? BusinessName { get; set; }
  public string? AddressLine1 { get; set; }
  public string? AddressLine2 { get; set; }
  public string? CityProvincePostal { get; set; }
  public string? Tel { get; set; }
  public string? Fax { get; set; }
  public string? HSTNumber { get; set; }
}

public class StatementSummaryDto
{
  public string Details { get; set; } = string.Empty;
  public decimal SubTotal { get; set; }
  public decimal Tax { get; set; }
  public decimal PaymentsMade { get; set; }
  public decimal UnusedCredit { get; set; }
  public decimal PleasePay { get; set; }
}

public class StatementSheetDto
{
  public StatementToDto To { get; set; } = new();
  public StatementFromDto From { get; set; } = new();
  public string StatementPeriod { get; set; } = string.Empty;
  public StatementSummaryDto Summary { get; set; } = new();
}
