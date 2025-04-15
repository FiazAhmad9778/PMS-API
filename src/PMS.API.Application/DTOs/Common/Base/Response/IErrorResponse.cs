using System.Collections.Generic;

namespace PMS.API.Application.DTOs.Common.Base.Response;

public interface IErrorResponse
{
  public bool IsSuccess { get; set; }
  public IEnumerable<string> Errors { get; set; }

}
