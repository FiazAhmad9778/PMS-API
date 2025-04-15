namespace PMS.API.Application.Features.Auth.DTO;

public class AccountResultDto
{
  public string? AccessToken { get; set; }
  public string? RefreshToken { get; set; }
  public string? Email { get; set; }
  public string? Name { get; set; }
  public string? RoleName { get; set; }
  public long UserId { get; set; }
  public string? ProfileImage { get; set; }
  public List<string>? Claims { get; set; }
}
