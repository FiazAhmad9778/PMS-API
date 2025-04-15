using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PMS.API.Web.Common;


public class ViewModelResult
{

  [JsonProperty("metadata")]
  public Dictionary<string, object>? Metadata { get; set; }


  [JsonProperty("errors")]
  public List<ServiceError>? Errors { get; set; }


  [JsonProperty("isPartialSuccess")]
  public bool IsPartialSuccess { get; set; }
}


public class ViewModelResult<T> : ViewModelResult
{

  [JsonProperty("result")]
  public T Result { get; set; }


  public ViewModelResult(T result) => Result = result;
}
