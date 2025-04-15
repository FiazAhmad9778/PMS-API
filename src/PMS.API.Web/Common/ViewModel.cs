using System;
using Newtonsoft.Json;

namespace PMS.API.Web.Common;

public class ViewModel
{
  [JsonProperty("metadata")]
  public Metadata? Metadata { get; set; }
}
