namespace PMS.API.Application.Features.Patients.DTO;

public class PatientDropdownDto
{
  public long Id { get; set; }
  public string? PatientId { get; set; }
  public required string Name { get; set; }
}

