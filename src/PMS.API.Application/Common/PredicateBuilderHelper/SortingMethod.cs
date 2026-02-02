using System.Globalization;
using System.Linq.Expressions;
using System.Text;

namespace PMS.API.Application.Common.PredicateBuilderHelper;
public static class SortingMethod
{
  public static IQueryable<TEntity> ApplySorting<TEntity>(this IQueryable<TEntity> query, string sortby, string sortDirection = "desc") where TEntity : class
  {
    if (string.IsNullOrWhiteSpace(sortby))
    {
      return query;
    }
    var sortBy = sortby; // ToPascalCase(sortby);
    var entityType = typeof(TEntity);
    var propertyName = sortBy.Trim();
    var property = entityType.GetProperty(propertyName);

    if (property == null)
    {
      throw new ArgumentException($"Property '{propertyName}' not found on type '{entityType.Name}'.");
    }

    var parameter = Expression.Parameter(entityType, "entity");
    var propertyAccess = Expression.MakeMemberAccess(parameter, property);
    var orderByExpression = Expression.Lambda(propertyAccess, parameter);

    if (sortDirection.Trim().ToLower(CultureInfo.CurrentCulture) == "desc")
    {
      var orderByDesc = Expression.Call(
          typeof(Queryable),
          "OrderByDescending",
          new[] { entityType, property.PropertyType },
          query.Expression,
          Expression.Quote(orderByExpression)
      );

      return query.Provider.CreateQuery<TEntity>(orderByDesc);
    }
    else
    {
      var orderByAsc = Expression.Call(
          typeof(Queryable),
          "OrderBy",
          new[] { entityType, property.PropertyType },
          query.Expression,
          Expression.Quote(orderByExpression)
      );

      return query.Provider.CreateQuery<TEntity>(orderByAsc);
    }
  }

  public static string ToPascalCase(this string input)
  {
    if (string.IsNullOrWhiteSpace(input))
    {
      return input;
    }

    string[] words = input.Split(new[] { ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);

    StringBuilder pascalCaseString = new StringBuilder();
    TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;

    foreach (string word in words)
    {
      pascalCaseString.Append(textInfo.ToTitleCase(word));
    }

    return pascalCaseString.ToString();

  }

  public static IEnumerable<TEntity> ApplyListSorting<TEntity>(this IEnumerable<TEntity> list, string sortby, string sortDirection = "desc") where TEntity : class
  {
    if (string.IsNullOrWhiteSpace(sortby))
    {
      return list;
    }

    var sortBy = sortby.Trim();
    var entityType = typeof(TEntity);
    var propertyName = char.ToUpper(sortBy[0]) + sortBy.Substring(1); // Ensure the property name is in PascalCase
    var property = entityType.GetProperty(propertyName);

    if (property == null)
    {
      throw new ArgumentException($"Property '{propertyName}' not found on type '{entityType.Name}'.");
    }

    var parameter = Expression.Parameter(entityType, "entity");
    var propertyAccess = Expression.MakeMemberAccess(parameter, property);
    var orderByExpression = Expression.Lambda<Func<TEntity, object>>(Expression.Convert(propertyAccess, typeof(object)), parameter);

    if (sortDirection.Trim().ToLower(CultureInfo.CurrentCulture) == "desc")
    {
      return list.AsQueryable().OrderByDescending(orderByExpression).ToList();
    }
    else
    {
      return list.AsQueryable().OrderBy(orderByExpression).ToList();
    }
  }

}

