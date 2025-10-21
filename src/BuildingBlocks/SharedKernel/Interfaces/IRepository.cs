namespace SharedKernel.Interfaces;

/// <summary>
/// Legacy repository interface - Consider using IBaseRepository from SharedKernel.Data instead
/// </summary>
public interface IRepository<TEntity, TId> where TEntity : IEntity<TId>
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TId> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(TId id, CancellationToken cancellationToken = default);
}

