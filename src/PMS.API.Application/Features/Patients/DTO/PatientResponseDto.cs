namespace PMS.API.Application.Features.Patients.DTO;

public class PatientResponseDto
{
  public long Id { get; set; }
  public string? PatientId { get; set; } // External ID from Kroll database (ARID)
  public required string Name { get; set; }
  public string? Address { get; set; }
  public string? DefaultEmail { get; set; }
  public string Status { get; set; } = "active";
  public DateTime CreatedDate { get; set; }
}

