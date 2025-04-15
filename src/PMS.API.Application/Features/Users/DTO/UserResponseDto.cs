namespace PMS.API.Application.Features.Users.DTO;
public class UserResponseDto
{
  public long Id { get; set; }
  public required string FirstName { get; set; }
  public required string LastName { get; set; }
  public required string Email { get; set; }
  public string? AvatarUrl { get; set; }
  public required string Username { get; set; }
  public string? PhoneNumber { get; set; }
  public string? Address { get; set; }
  public long RoleId { get; set; }
  public string? RoleName { get; set; }
  public DateTimeOffset CreatedDate { get; set; }
  public string? SignatureBase64 { get; set; }
}
