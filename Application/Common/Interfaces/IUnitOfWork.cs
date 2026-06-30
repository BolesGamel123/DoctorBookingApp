namespace Application.Common.Interfaces;

/// <summary>
/// Unit of Work interface to manage repositories and commit transactions.
/// </summary>
public interface IUnitOfWork
{
    IRepository<T> Repository<T>() where T : class;
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
