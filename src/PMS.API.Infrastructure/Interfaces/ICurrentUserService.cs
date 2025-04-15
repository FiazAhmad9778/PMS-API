namespace PMS.API.Infrastructure.Interfaces;

public interface ICurrentUserService
{
  long? UserId { get; }
  string? Email { get; }
  long EnsureGetUserId();
  string EnsureGetUserEmail();
}


public class DefaultCurrentUserService : ICurrentUserService
{
  public virtual long? UserId => null;
  public virtual string? Email => null;

  public virtual long EnsureGetUserId()
  {
    if (UserId.HasValue)
    {
      return UserId.Value;
    }

    throw new UnauthorizedAccessException();
  }

  public string EnsureGetUserEmail()
  {
    if (!string.IsNullOrEmpty(Email))
    {
      return Email;
    }

    throw new UnauthorizedAccessException();
  }
}
