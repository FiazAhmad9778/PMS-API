using System;
using System.Collections.Generic;

namespace PMS.API.Web.Common;

public class Metadata : Dictionary<string, object>
{
  public Metadata() { }

  public Metadata(IDictionary<string, object> metadata)
  {
    foreach (var (key, value) in metadata)
      Add(key, value);
  }
}
