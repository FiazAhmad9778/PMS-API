namespace PMS.API.Application.Features.Organizations.DTO;

public class OrganizationResponseDto
{
  public long Id { get; set; }
  public required long OrganizationExternalId { get; set; }
  public required string Name { get; set; }
  public long[]? WardIds { get; set; }
  public required string Address { get; set; }
  public string? DefaultEmail { get; set; }
  public DateTime CreatedDate { get; set; }
}

