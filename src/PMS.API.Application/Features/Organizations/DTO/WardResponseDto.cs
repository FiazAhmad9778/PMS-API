namespace PMS.API.Application.Features.Organizations.DTO;

public class WardResponseDto
{
  public long Id { get; set; }
  public string? Name { get; set; }
  public long? ExternalId { get; set; } // External ID from Kroll database
}

public class WardPageResponseDTO
{
  public long Id { get; set; }
  public required string Name { get; set; }
  public required long ExternalId { get; set; }
  public long? OrganizationId { get; set; }
  public string? OrganizationName { get; set; }
  public DateTime CreatedDate { get; set; }
  public DateTime? ModifiedDate { get; set; }
}

