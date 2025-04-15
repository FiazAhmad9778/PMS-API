using System.Collections.Generic;

namespace PMS.API.Core.DTOs.Base.Response;

public interface IErrorResponse
{
  public bool IsSuccess { get; set; }
  public IEnumerable<string> Errors { get; set; }

}
