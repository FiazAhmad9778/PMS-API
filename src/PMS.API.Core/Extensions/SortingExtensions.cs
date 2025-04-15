using System.Linq.Expressions;
using System.Reflection;
using PMS.API.Core.Domain.Interfaces;

public static class SortingExtensions
{
  public static IQueryable<TEntity> OrderByColumn<TEntity>(
        this IQueryable<TEntity> query,
        string columnName,
        bool ascending = true)
  {
    var parameter = Expression.Parameter(typeof(TEntity), "x");
    var property = Expression.Property(parameter, columnName);
    var lambda = Expression.Lambda<Func<TEntity, dynamic>>(property, parameter);

    if (ascending)
    {
      return query.OrderBy(lambda);
    }
    else
    {
      return query.OrderByDescending(lambda);
    }
  }
  public static IQueryable<T> SortByProperty<T>(
      this IQueryable<T> collection,
      string propertyPath,
      bool ascending = true) where T : IAuditEntity
  {
    if (string.IsNullOrEmpty(propertyPath))
    {
      propertyPath = "CreatedDate";
    }
    PropertyInfo? propertyInfo = null;
    Type entityType = typeof(T);

    // Navigate through property path to get the final property
    foreach (string propertyName in propertyPath.Split('.'))
    {
      propertyInfo = entityType!.GetProperty(propertyName)!;

      if (propertyInfo == null)
      {
        throw new ArgumentException($"Property '{propertyName}' not found in type '{entityType.Name}'.");
      }

      entityType = propertyInfo.PropertyType;
    }

    if (ascending)
    {
      return collection.OrderBy(entity => propertyInfo!.GetValue(GetPropertyValue(entity!, propertyPath)));
    }
    else
    {
      return collection.OrderByDescending(entity => propertyInfo!.GetValue(GetPropertyValue(entity!, propertyPath)));
    }
  }

  private static object GetPropertyValue(object entity, string propertyPath)
  {
    foreach (string propertyName in propertyPath.Split('.'))
    {
      PropertyInfo propertyInfo = entity!.GetType()!.GetProperty(propertyName)!;

      if (propertyInfo == null)
      {
        throw new ArgumentException($"Property '{propertyName}' not found in type '{entity.GetType().Name}'.");
      }

      entity = propertyInfo!.GetValue(entity)!;
    }

    return entity;
  }
}
