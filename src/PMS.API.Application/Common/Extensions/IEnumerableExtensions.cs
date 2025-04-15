namespace PMS.API.Application.Common.Extensions;
public static class IEnumerableExtensions
{
  public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
  {
    return enumerable == null || !enumerable.Any();
  }
  public static bool IsNullOrEmpty<T>(this List<T> list)
  {
    return list == null || list.Count == 0;
  }
}
