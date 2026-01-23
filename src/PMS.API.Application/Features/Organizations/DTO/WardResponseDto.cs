namespace PMS.API.Application.Features.Organizations.DTO;

public class WardResponseDto
{
  public long Id { get; set; }
  public required string Name { get; set; }
  public string? ExternalId { get; set; } // External ID from Kroll database
}

