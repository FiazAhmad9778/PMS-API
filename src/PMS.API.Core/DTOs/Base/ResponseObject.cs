using System;
using System.Collections.Generic;

namespace PMS.API.Core.DTOs.Base;

public class ResponseObject<T>
{
  public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();

  public T Value { get; }

  public ResponseObject(T value)
  {
    Value = value;
  }

  public void AddMetadata(string key, object value)
  {
    Metadata.Add(key, value);
  }
}
