using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PMS.API.SharedKernel.Interfaces;

namespace PMS.API.Core.Domain.Interfaces;

public interface IARDashboardRepository<T> : IReadRepository<T> where T : class, IAggregateRoot
{
  Task<T> AddAsync(T model);
  Task DeleteAsync(long id);
  Task<T?> GetByIdAsync(long id);
  Task UpdateAsync(T model);
  Task UpdateRangeAsync(List<T> model);
  IQueryable<T> Get();
  Task<T?> FirstAsync(Expression<Func<T, bool>>? predicate = null, Func<IIncludable<T>, IIncludable>? includes = null);
  IQueryable<T> GetWithTracking();
  Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
  EntityEntry<T> Entry(T entity);
}

