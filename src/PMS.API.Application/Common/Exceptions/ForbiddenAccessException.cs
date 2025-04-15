namespace PMS.API.Application.Common.Exceptions;

public class ForbiddenAccessException : Exception
{
  public ForbiddenAccessException(string? message = null) : base(message) { }
}
