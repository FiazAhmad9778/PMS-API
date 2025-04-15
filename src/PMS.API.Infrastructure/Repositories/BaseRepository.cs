using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PMS.API.Core.Domain.Interfaces;
using PMS.API.Infrastructure.Data;

namespace PMS.API.Infrastructure.Repositories;
public class BaseRepository<T> : IRepository<T> where T : class
{
  protected readonly AppDbContext _context;

  public BaseRepository(AppDbContext context)
  {
    _context = context;
  }

  public void Add(T entity)
  {
    _context.Set<T>().Add(entity);
  }

  public void AddRange(IEnumerable<T> entities)
  {
    _context.Set<T>().AddRange(entities);
  }

  public IEnumerable<T> Find(Expression<Func<T, bool>> expression)
  {
    return _context.Set<T>().Where(expression);
  }

  public async Task<IEnumerable<T>> GetAllAsync()
  {
    return await _context.Set<T>().ToListAsync();
  }

  public virtual async Task<T?> GetByIdAsync(string id)
  {
    return await _context.Set<T>().FindAsync(new Guid(id));
  }

  public void Remove(T entity)
  {
    _context.Set<T>().Remove(entity);
  }

  public void RemoveRange(IEnumerable<T> entities)
  {
    _context.Set<T>().RemoveRange(entities);
  }

  public async virtual Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
  {
    return await _context.Set<T>().Where(predicate).ToListAsync();
  }

  Task<List<T>> IRepository<T>.GetAsync(IList<string> ids)
  {
    throw new NotImplementedException();
  }

  public async Task<int> CountAsync()
  {
    return await _context.Set<T>().CountAsync();
  }

  async Task IRepository<T>.AddAsync(T entity)
  {
    await _context.Set<T>().AddAsync(entity);
  }

  async Task IRepository<T>.AddRange(List<T> entities)
  {
    await _context.Set<T>().AddRangeAsync(entities);
  }

  void IRepository<T>.Update(T entity)
  {
    _context.Set<T>().Update(entity);
  }

  Task IRepository<T>.DeleteAsync(T entity)
  {
    throw new NotImplementedException();
  }

  Task IRepository<T>.UpsertAsync(T entity)
  {
    throw new NotImplementedException();
  }

  Task<List<T>> IRepository<T>.FindAsync(Expression<Func<T, bool>> predicate)
  {
    throw new NotImplementedException();
  }

  public Task<IList<T>> GetAllAsync(int skip, int take)
  {
    throw new NotImplementedException();
  }
}
