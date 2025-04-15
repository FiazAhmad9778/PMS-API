using System;
using System.Collections.Generic;
using System.Linq;

namespace PMS.API.Core.DTOs.Base;
public class ObjectSetResponse<T> : ObjectResponse
{
  public ObjectSetResponse() { }
  public int Count => Results != null ? Results.Count : 0;
  public IReadOnlyCollection<ResponseObject<T>>? Results { get; set; }
  public ObjectSetResponse(IEnumerable<ResponseObject<T>> results) { Results = results?.ToList() ?? new List<ResponseObject<T>>(); }
}
