namespace PMS.API.Application.Common.Context.Models;

public class CurrentUserContext
{
  public CurrentUserContext()
  {
    NotMappedRoles = new List<string>();
  }

  public long Id { get; set; }
  public string? FirstName { get; set; }
  public string? LastName { get; set; }
  public string? UserName { get; set; }
  public int UserTypeId { get; set; }

  public virtual List<string> NotMappedRoles { get; set; }
}
