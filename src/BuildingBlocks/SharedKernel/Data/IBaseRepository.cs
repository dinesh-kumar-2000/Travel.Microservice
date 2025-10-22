using SharedKernel.Models;

namespace SharedKernel.Data;

/// <summary>
/// Enhanced base repository interface with common CRUD operations and pagination support
/// </summary>
public interface IBaseRepository<TEntity, TId> where TEntity : class
{
    /// <summary>
    /// Gets an entity by its identifier
    /// </summary>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all entities
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets entities with pagination
    /// </summary>
    Task<PagedResult<TEntity>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds a new entity
    /// </summary>
    Task<TId> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing entity
    /// </summary>
    Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes an entity by its identifier
    /// </summary>
    Task<bool> DeleteAsync(TId id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if an entity exists
    /// </summary>
    Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the total count of entities
    /// </summary>
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Base repository interface with soft delete support
/// </summary>
public interface ISoftDeletableRepository<TEntity, TId> : IBaseRepository<TEntity, TId> where TEntity : class
{
    /// <summary>
    /// Soft deletes an entity (marks as deleted without removing from database)
    /// </summary>
    Task<bool> SoftDeleteAsync(TId id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Restores a soft-deleted entity
    /// </summary>
    Task<bool> RestoreAsync(TId id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all entities including soft-deleted ones
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Base repository interface for tenant-specific entities
/// </summary>
public interface ITenantBaseRepository<TEntity, TId> : IBaseRepository<TEntity, TId> where TEntity : class
{
    /// <summary>
    /// Gets an entity by its identifier for a specific tenant
    /// </summary>
    Task<TEntity?> GetByIdAsync(TId id, Guid tenantId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all entities for a specific tenant
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets entities with pagination for a specific tenant
    /// </summary>
    Task<PagedResult<TEntity>> GetPagedByTenantAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the total count of entities for a specific tenant
    /// </summary>
    Task<int> CountByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

