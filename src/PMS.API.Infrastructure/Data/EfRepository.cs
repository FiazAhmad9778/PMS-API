using System.Linq.Expressions;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PMS.API.Core.Domain.Entities.Identity;
using PMS.API.Core.Domain.Interfaces;
using PMS.API.Core.Enums;
using PMS.API.Core.Extensions;
using PMS.API.SharedKernel.Interfaces;

namespace PMS.API.Infrastructure.Data;

public class EfRepository<T> : RepositoryBase<T>, IEfRepository<T>, SharedKernel.Interfaces.IRepository<T> where T : class, IAggregateRoot
{
  private readonly AppDbContext _dbContext;

  public EfRepository(AppDbContext dbContext) : base(dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<T> AddAsync(T model)
  {
    return await base.AddAsync(model);
  }

  public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
  {
    return await base.AddRangeAsync(entities);
  }

  public async Task DeleteAsync(long id)
  {
    var model = await base.GetByIdAsync(id);
    await base.DeleteAsync(model!);
  }

  public async Task<T?> GetByIdAsync(long id)
  {
    return await base.GetByIdAsync(id);
  }

  public IQueryable<T> Get()
  {
    return _dbContext.Set<T>().AsNoTracking();
  }

  public IQueryable<T> GetWithTracking()
  {
    return _dbContext.Set<T>();
  }

  public EntityEntry<T> Entry(T entity)
  {
    return _dbContext.Entry(entity);
  }


  public async Task UpdateAsync(T model)
  {
    await base.UpdateAsync(model);
  }
  public async Task UpdateRangeAsync(List<T> model)
  {
    await base.UpdateRangeAsync(model);
  }

  public virtual T? First(Expression<Func<T, bool>>? predicate = null,
                                 Func<IIncludable<T>, IIncludable>? includes = null)
  {
    var dbSet = _dbContext.Set<T>() as IQueryable<T>;

    if (includes != null)
    {
      dbSet = dbSet.IncludeMultiple(includes);
    }

    return predicate == null
               ? dbSet.FirstOrDefault()
               : dbSet.FirstOrDefault(predicate);
  }

  public virtual Task<T?> LastAsync(Expression<Func<T, bool>>? predicate = null,
                                 Func<IIncludable<T>, IIncludable>? includes = null)
  {
    var dbSet = _dbContext.Set<T>() as IQueryable<T>;

    if (includes != null)
    {
      dbSet = dbSet.IncludeMultiple(includes);
    }

    return predicate == null
               ? dbSet.LastOrDefaultAsync()
               : dbSet.LastOrDefaultAsync(predicate);
  }

  public virtual Task<T?> FirstAsync(Expression<Func<T, bool>>? predicate = null,
                            Func<IIncludable<T>, IIncludable>? includes = null)
  {
    var dbSet = _dbContext.Set<T>() as IQueryable<T>;

    if (includes != null)
    {
      dbSet = dbSet.IncludeMultiple(includes);
    }

    return predicate == null
               ? dbSet.FirstOrDefaultAsync()
               : dbSet.FirstOrDefaultAsync(predicate);
  }

  public async Task<List<User>> GetUsersByUserTypes(List<AppUserTypeEnums> userTypes)
  {
    return await _dbContext.Users.Where(x => userTypes.Contains(x.UserType)).ToListAsync();
  }
  public async Task<User?> GetSuperAdmin()
  {
    return await _dbContext.Users.FirstOrDefaultAsync(x => x.Email!.Equals("superadmin@rout-d.com"));
  }
}
