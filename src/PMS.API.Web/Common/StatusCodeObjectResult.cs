using System;
using Microsoft.AspNetCore.Mvc;

namespace PMS.API.Web.Common;

public class StatusCodeObjectResult : ObjectResult
{
  public StatusCodeObjectResult(int statusCode, object value) : base(value) => StatusCode = statusCode;

}
