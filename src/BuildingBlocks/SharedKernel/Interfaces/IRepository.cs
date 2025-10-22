using SharedKernel.Data;

namespace SharedKernel.Interfaces;

/// <summary>
/// Legacy repository interface - now inherits from IBaseRepository for consistency
/// Consider using IBaseRepository from SharedKernel.Data directly for new code
/// </summary>
[Obsolete("Use IBaseRepository from SharedKernel.Data instead. This interface is kept for backward compatibility and will be removed in a future version.")]
public interface IRepository<TEntity, TId> : IBaseRepository<TEntity, TId> 
    where TEntity : class, IEntity<TId>
{
    // All methods now inherited from IBaseRepository
    // This interface adds the IEntity<TId> constraint for domain entities
}

