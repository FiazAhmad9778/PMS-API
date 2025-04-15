using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.API.Core.DTOs.Base.Request;

public class BaseCreateRequestDto : BaseRequestDto
{
  public BaseCreateRequestDto()
  {
    CreatedDate = DateTime.UtcNow;
    IsActive = true;
  }

  public DateTime CreatedDate { get; set; }
  public bool IsActive { get; set; }
}
