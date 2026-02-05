namespace PMS.API.Application.Features.Organizations.DTO;

public class OrganizationResponseDto
{
  public long Id { get; set; }
  public long OrganizationExternalId { get; set; }
  public string? Name { get; set; }
  public long[]? WardIds { get; set; }
  public List<WardResponseDto> Wards { get; set; } = new List<WardResponseDto>();
  public string? Address { get; set; }
  public string? DefaultEmail { get; set; }
  public DateTime CreatedDate { get; set; }
  public DateTime? ModifiedDate { get; set; }
  public string? InvoicePath { get; set; }
}

