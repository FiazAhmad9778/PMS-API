using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace PMS.API.Web.Common;
public class ViewModelResultSet<T> : ViewModelResult
    where T : ViewModel
{

  [JsonProperty("count")]
  public int Count => Results.Count;


  [JsonProperty("results")]
  public List<ViewModelResult<T>> Results { get; set; }


  public ViewModelResultSet(IEnumerable<ViewModelResult<T>> viewModels) => Results = viewModels.ToList();
}
