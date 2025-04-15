using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PMS.API.Web.Common;


public class ServiceError
{
  [JsonProperty("code")]
  public int Code { get; set; }

  [JsonProperty("message")]
  public string? Message { get; set; }


  [JsonProperty("trace")]
  public List<ServiceError>? Trace { get; set; }
}
