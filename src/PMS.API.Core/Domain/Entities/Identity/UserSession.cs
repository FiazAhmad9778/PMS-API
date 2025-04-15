namespace PMS.API.Core.Domain.Entities.Identity;

public class UserSession
{
  public int Id { get; set; }

  public int UserId { get; set; }

  public virtual User? User { get; set; }

  public string? RefreshToken { get; set; }

}
