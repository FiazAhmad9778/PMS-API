using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PMS.API.Core.Domain.Entities.Identity;
using PMS.API.Core.Enums;
using PMS.API.SharedKernel.Interfaces;

namespace PMS.API.Core.Domain.Interfaces;
public interface IEfRepository<T> : IReadRepository<T> where T : class, IAggregateRoot
{
  Task<T> AddAsync(T model);
  Task DeleteAsync(long id);
  Task<T?> GetByIdAsync(long id);
  Task UpdateAsync(T model);
  Task UpdateRangeAsync(List<T> model);
  IQueryable<T> Get();
  T? First(Expression<Func<T, bool>>? predicate = null, Func<IIncludable<T>, IIncludable>? includes = null);
  Task<T?> FirstAsync(Expression<Func<T, bool>>? predicate = null, Func<IIncludable<T>, IIncludable>? includes = null);
  Task<T?> LastAsync(Expression<Func<T, bool>>? predicate = null, Func<IIncludable<T>, IIncludable>? includes = null);
  Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
  IQueryable<T> GetWithTracking();
  Task<List<User>> GetUsersByUserTypes(List<AppUserTypeEnums> userTypes);
  Task<User?> GetSuperAdmin();
  EntityEntry<T> Entry(T entity);
}
