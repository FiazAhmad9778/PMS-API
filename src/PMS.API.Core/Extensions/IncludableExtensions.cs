using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PMS.API.Core.Domain.Interfaces;
using PMS.API.Core.Extensions;

namespace PMS.API.Core.Extensions;

public static class IncludableExtensions
{
  public static IQueryable<T> IncludeMultiple<T>(this IQueryable<T> query, Func<IIncludable<T>, IIncludable> includes) where T : class
  {
    if (includes == null)
      return query;

    var includable = (Includable<T>)includes(new Includable<T>(query));
    return includable.Input;
  }

  public static IIncludable<TEntity, TProperty> Include<TEntity, TProperty>(
      this IIncludable<TEntity> includes,
      Expression<Func<TEntity, TProperty>> propertySelector)
      where TEntity : class
  {
    var result = ((Includable<TEntity>)includes).Input
        .Include(propertySelector);
    return new Includable<TEntity, TProperty>(result);
  }

  public static IIncludable<TEntity, TOtherProperty>
      ThenInclude<TEntity, TOtherProperty, TProperty>(
          this IIncludable<TEntity, TProperty> includes,
          Expression<Func<TProperty, TOtherProperty>> propertySelector)
      where TEntity : class
  {
    var result = ((Includable<TEntity, TProperty>)includes)
        .IncludableInput.ThenInclude(propertySelector);
    return new Includable<TEntity, TOtherProperty>(result);
  }

  public static IIncludable<TEntity, TOtherProperty>
      ThenInclude<TEntity, TOtherProperty, TProperty>(
          this IIncludable<TEntity, IEnumerable<TProperty>> includes,
          Expression<Func<TProperty, TOtherProperty>> propertySelector)
      where TEntity : class
  {
    var result = ((Includable<TEntity, IEnumerable<TProperty>>)includes)
        .IncludableInput.ThenInclude(propertySelector);
    return new Includable<TEntity, TOtherProperty>(result);
  }

  public static IIncludable<TEntity, TOtherProperty>
      ThenInclude<TEntity, TOtherProperty, TProperty>(
          this IIncludable<TEntity, ICollection<TProperty>> includes,
          Expression<Func<TProperty, TOtherProperty>> propertySelector)
      where TEntity : class
  {
    var result = ((Includable<TEntity, ICollection<TProperty>>)includes)
        .IncludableInput.ThenInclude(propertySelector);
    return new Includable<TEntity, TOtherProperty>(result);
  }
}
