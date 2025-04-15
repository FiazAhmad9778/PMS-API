using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PMS.API.Web.Common;


public class PagedViewModelResultSet<T> : ViewModelResultSet<T>
    where T : ViewModel
{

  [JsonProperty("page")]
  public int Page { get; set; }

  [JsonProperty("limit")]
  public int PageSize { get; set; }

  [JsonProperty("total")]
  public int TotalCount { get; set; }

  public PagedViewModelResultSet(IEnumerable<ViewModelResult<T>> viewModels, int page, int pageSize, int totalCount)
      : base(viewModels)
  {
    Page = page;
    PageSize = pageSize;
    TotalCount = totalCount;
  }
}
