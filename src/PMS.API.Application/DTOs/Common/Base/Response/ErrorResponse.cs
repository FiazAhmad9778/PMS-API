using PMS.API.Application.Common.Enums;

namespace PMS.API.Application.DTOs.Common.Base.Response;

public class ErrorResponse : IErrorResponse
{
  public ErrorResponse()
  {
    Errors = new List<string>();
    ValidationErrors = new List<ValidationError>();
    IsSuccess = true;
  }
  public bool IsSuccess { get; set; }
  public IEnumerable<string> Errors { get; set; }
  public IEnumerable<ValidationError> ValidationErrors { get; set; }
  public string? Message { get; set; }
  public PMSErrorEnums? ErrorId { get; set; }
}
public class ValidationError
{
  private string name = string.Empty;
  public required string Name
  {
    get
    {
      if (string.IsNullOrEmpty(name)) return name;

      return Char.ToLowerInvariant(name[0]) + name.Substring(1);
    }
    set
    {
      name = value;
    }
  }
  public required string Message { get; set; }
}
