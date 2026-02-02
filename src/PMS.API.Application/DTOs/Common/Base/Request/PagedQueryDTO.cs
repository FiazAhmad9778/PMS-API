using PMS.API.Application.Common.PredicateBuilderHelper;

namespace PMS.API.Application.DTOs.Common.Base.Request;
public class PagedQueryDTO
{
  public virtual int PageSize { get; set; } = 50;
  public int PageNumber { get; set; } = 1;
  public string? OrderBy { get; set; } = "CreatedDate";
  public bool SortByAscending { get; set; } = false;
  public string? SearchKeyword { get; set; }
}
