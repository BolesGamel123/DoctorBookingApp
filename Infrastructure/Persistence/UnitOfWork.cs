using Application.Common.Interfaces;

namespace Infrastructure.Persistence;

/// <summary>
/// Concrete implementation of the Unit of Work pattern using EF Core AppDbContext.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly AppDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T);

        if (!_repositories.TryGetValue(type, out var repository))
        {
            repository = new EfRepository<T>(_context);
            _repositories.Add(type, repository);
        }

        return (IRepository<T>)repository;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return _context.SaveChangesAsync(ct);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
