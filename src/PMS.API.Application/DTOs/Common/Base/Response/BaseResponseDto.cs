using System;

namespace PMS.API.Application.DTOs.Common.Base.Response;

public class BaseResponseDto : ErrorResponse
{
  public BaseResponseDto()
  {

  }
  public int Id { get; set; }
  public int CreatedById { get; set; }
  public int ModifiedById { get; set; }

  public DateTime CreatedDate { get; set; }
  public DateTime? ModifiedDate { get; set; }

  public bool IsActive { get; set; }
}
