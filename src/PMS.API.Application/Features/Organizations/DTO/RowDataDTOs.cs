namespace PMS.API.Application.Features.Organizations.DTO;
public class PmsOrgWardRow
{
  public long OrganizationId { get; init; }
  public string? OrganizationName { get; init; }
  public string? Address { get; init; }
  public long? WardId { get; init; }
  public string? WardName { get; init; }
}
public class PmsPatientRow
{
  public long PatientExternalId { get; set; }
  public string PatientName { get; set; } = string.Empty;
  public string? Email { get; set; }
  public string? Address { get; set; }
  public long WardExternalId { get; set; }
}
