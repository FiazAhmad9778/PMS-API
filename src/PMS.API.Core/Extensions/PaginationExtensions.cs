namespace PMS.API.Core.Extensions;
public static class PaginationExtensions
{
  public static IQueryable<T> Paginate<T>(this IQueryable<T> query, int pageNumber = 1, int pageSize = 10)
  {
    if (pageNumber <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than 0.");
    }

    if (pageSize <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than 0.");
    }

    int skip = (pageNumber - 1) * pageSize;
    return query.Skip(skip).Take(pageSize);
  }
}
