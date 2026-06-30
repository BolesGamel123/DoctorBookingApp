using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

/// <summary>
/// Concrete EF Core implementation of the generic IRepository interface.
/// </summary>
public sealed class EfRepository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _context;

    public EfRepository(AppDbContext context) => _context = context;

    public async Task<T?> GetByIdAsync(long id, CancellationToken ct = default)
        => await _context.Set<T>().FindAsync([id], ct);

    public async Task<List<T>> GetAllAsync(CancellationToken ct = default)
        => await _context.Set<T>().ToListAsync(ct);

    public IQueryable<T> Query()
        => _context.Set<T>();

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await _context.Set<T>().AddAsync(entity, ct);

    public void Update(T entity)
        => _context.Set<T>().Update(entity);

    public void Remove(T entity)
        => _context.Set<T>().Remove(entity);
}
