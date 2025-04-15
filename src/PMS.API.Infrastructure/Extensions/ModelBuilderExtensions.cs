using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace PMS.API.Infrastructure.Extensions;

public static class ModelBuilderExtensions
{
  public static void AddQueryFilterToAllEntitiesAssignableFrom<T>(this ModelBuilder modelBuilder,
      Expression<Func<T, bool>> expression)
  {
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
      if (!typeof(T).IsAssignableFrom(entityType.ClrType))
        continue;

      var parameterType = Expression.Parameter(entityType.ClrType);
      var expressionFilter = ReplacingExpressionVisitor.Replace(
          expression.Parameters.Single(), parameterType, expression.Body);

      var currentQueryFilter = entityType.GetQueryFilter();
      if (currentQueryFilter != null)
      {
        var currentExpressionFilter = ReplacingExpressionVisitor.Replace(
            currentQueryFilter.Parameters.Single(), parameterType, currentQueryFilter.Body);
        expressionFilter = Expression.AndAlso(currentExpressionFilter, expressionFilter);
      }

      var lambdaExpression = Expression.Lambda(expressionFilter, parameterType);
      entityType.SetQueryFilter(lambdaExpression);
    }
  }
}
