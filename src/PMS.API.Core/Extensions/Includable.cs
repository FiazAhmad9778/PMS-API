using Microsoft.EntityFrameworkCore.Query;
using PMS.API.Core.Domain.Interfaces;

namespace PMS.API.Core.Extensions;

public class Includable<TEntity> : IIncludable<TEntity> where TEntity : class
{
  public IQueryable<TEntity> Input { get; }

  public Includable(IQueryable<TEntity> queryable)
  {
    // C# 7 syntax, just rewrite it "old style" if you do not have Visual Studio 2017
    Input = queryable ?? throw new ArgumentNullException(nameof(queryable));
  }
}


public class Includable<TEntity, TProperty> :
    Includable<TEntity>, IIncludable<TEntity, TProperty>
    where TEntity : class
{
  public IIncludableQueryable<TEntity, TProperty> IncludableInput { get; }

  public Includable(IIncludableQueryable<TEntity, TProperty> queryable) :
      base(queryable)
  {
    IncludableInput = queryable;
  }
}
