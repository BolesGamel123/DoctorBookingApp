using System.Linq.Expressions;

namespace Application.Common.Interfaces;

/// <summary>
/// Generic repository interface for basic CRUD operations.
/// This allows feature handlers to avoid direct coupling to EF Core DbContext.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<List<T>> GetAllAsync(CancellationToken ct = default);
    IQueryable<T> Query();
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
}
