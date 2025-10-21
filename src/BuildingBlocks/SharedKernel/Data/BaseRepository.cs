using System.Data;
using Dapper;
using SharedKernel.Models;
using Microsoft.Extensions.Logging;

namespace SharedKernel.Data;

/// <summary>
/// Base repository implementation using Dapper
/// Provides common CRUD operations for all repositories
/// </summary>
public abstract class BaseRepository<TEntity, TId> : IBaseRepository<TEntity, TId> where TEntity : class
{
    protected readonly IDapperContext _context;
    protected readonly ILogger _logger;
    protected abstract string TableName { get; }
    protected abstract string IdColumnName { get; }

    protected BaseRepository(IDapperContext context, ILogger logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected IDbConnection CreateConnection() => _context.CreateConnection();

    public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = $"SELECT * FROM {TableName} WHERE {IdColumnName} = @Id";
        return await connection.QueryFirstOrDefaultAsync<TEntity>(sql, new { Id = id });
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = $"SELECT * FROM {TableName}";
        return await connection.QueryAsync<TEntity>(sql);
    }

    public virtual async Task<PagedResult<TEntity>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        
        var offset = (page - 1) * pageSize;
        var countSql = $"SELECT COUNT(*) FROM {TableName}";
        var dataSql = $"SELECT * FROM {TableName} ORDER BY {IdColumnName} LIMIT @PageSize OFFSET @Offset";

        var totalCount = await connection.ExecuteScalarAsync<int>(countSql);
        var items = await connection.QueryAsync<TEntity>(dataSql, new { PageSize = pageSize, Offset = offset });

        return new PagedResult<TEntity>(
            items.ToList(),
            totalCount,
            page,
            pageSize
        );
    }

    public abstract Task<TId> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    public abstract Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    public virtual async Task<bool> DeleteAsync(TId id, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = $"DELETE FROM {TableName} WHERE {IdColumnName} = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public virtual async Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = $"SELECT EXISTS(SELECT 1 FROM {TableName} WHERE {IdColumnName} = @Id)";
        return await connection.ExecuteScalarAsync<bool>(sql, new { Id = id });
    }

    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = $"SELECT COUNT(*) FROM {TableName}";
        return await connection.ExecuteScalarAsync<int>(sql);
    }
}

/// <summary>
/// Base repository implementation for tenant-specific entities
/// </summary>
public abstract class TenantBaseRepository<TEntity, TId> : BaseRepository<TEntity, TId>, ITenantBaseRepository<TEntity, TId> 
    where TEntity : class
{
    protected abstract string TenantIdColumnName { get; }

    protected TenantBaseRepository(IDapperContext context, ILogger logger) : base(context, logger)
    {
    }

    public virtual async Task<TEntity?> GetByIdAsync(TId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = $"SELECT * FROM {TableName} WHERE {IdColumnName} = @Id AND {TenantIdColumnName} = @TenantId";
        return await connection.QueryFirstOrDefaultAsync<TEntity>(sql, new { Id = id, TenantId = tenantId });
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = $"SELECT * FROM {TableName} WHERE {TenantIdColumnName} = @TenantId";
        return await connection.QueryAsync<TEntity>(sql, new { TenantId = tenantId });
    }

    public virtual async Task<PagedResult<TEntity>> GetPagedByTenantAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        
        var offset = (page - 1) * pageSize;
        var countSql = $"SELECT COUNT(*) FROM {TableName} WHERE {TenantIdColumnName} = @TenantId";
        var dataSql = $"SELECT * FROM {TableName} WHERE {TenantIdColumnName} = @TenantId ORDER BY {IdColumnName} LIMIT @PageSize OFFSET @Offset";

        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, new { TenantId = tenantId });
        var items = await connection.QueryAsync<TEntity>(dataSql, new { TenantId = tenantId, PageSize = pageSize, Offset = offset });

        return new PagedResult<TEntity>(
            items.ToList(),
            totalCount,
            page,
            pageSize
        );
    }

    public virtual async Task<int> CountByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = $"SELECT COUNT(*) FROM {TableName} WHERE {TenantIdColumnName} = @TenantId";
        return await connection.ExecuteScalarAsync<int>(sql, new { TenantId = tenantId });
    }
}

