using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;


namespace PMS.API.Core.Domain.Interfaces;

public interface IRepository<T> where T : class
{
  Task<T?> GetByIdAsync(string id);
  Task<List<T>> GetAsync(IList<string> ids);
  Task<IList<T>> GetAllAsync(int skip, int take);
  Task AddAsync(T entity);
  Task AddRange(List<T> entities);
  void Update(T entity);
  Task DeleteAsync(T entity);
  Task UpsertAsync(T entity);
  Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);
  Task<int> CountAsync();
}
