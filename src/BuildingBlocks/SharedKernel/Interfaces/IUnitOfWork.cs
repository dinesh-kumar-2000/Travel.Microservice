using System.Data;

namespace SharedKernel.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IDbTransaction BeginTransaction();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

