using System.Data;
using Dapper;
using SharedKernel.Models;
using SharedKernel.Utilities;
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
        var sql = SqlQueryBuilder.SelectById(TableName, IdColumnName);
        return await connection.QueryFirstOrDefaultAsync<TEntity>(sql, new { Id = id });
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = SqlQueryBuilder.SelectAll(TableName);
        return await connection.QueryAsync<TEntity>(sql);
    }

    public virtual async Task<PagedResult<TEntity>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        
        var offset = (page - 1) * pageSize;
        var countSql = SqlQueryBuilder.CountAll(TableName);
        var dataSql = SqlQueryBuilder.SelectPaged(TableName, IdColumnName);

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
        var sql = SqlQueryBuilder.DeleteById(TableName, IdColumnName);
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public virtual async Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = SqlQueryBuilder.ExistsById(TableName, IdColumnName);
        return await connection.ExecuteScalarAsync<bool>(sql, new { Id = id });
    }

    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = SqlQueryBuilder.CountAll(TableName);
        return await connection.ExecuteScalarAsync<int>(sql);
    }
}

/// <summary>
/// Base repository implementation with soft delete support
/// </summary>
public abstract class SoftDeletableRepository<TEntity, TId> : BaseRepository<TEntity, TId>, ISoftDeletableRepository<TEntity, TId>
    where TEntity : class
{
    private readonly IDateTimeProvider _dateTimeProvider;
    protected virtual string IsDeletedColumnName => "IsDeleted";
    protected virtual string DeletedAtColumnName => "DeletedAt";
    protected virtual string DeletedByColumnName => "DeletedBy";

    protected SoftDeletableRepository(IDapperContext context, ILogger logger, IDateTimeProvider dateTimeProvider) 
        : base(context, logger)
    {
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public virtual async Task<bool> SoftDeleteAsync(TId id, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = $"UPDATE {TableName} SET {IsDeletedColumnName} = true, {DeletedAtColumnName} = @DeletedAt WHERE {IdColumnName} = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, DeletedAt = _dateTimeProvider.UtcNow });
        return rowsAffected > 0;
    }

    public virtual async Task<bool> RestoreAsync(TId id, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = $"UPDATE {TableName} SET {IsDeletedColumnName} = false, {DeletedAtColumnName} = NULL, {DeletedByColumnName} = NULL WHERE {IdColumnName} = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = SqlQueryBuilder.SelectAll(TableName);
        return await connection.QueryAsync<TEntity>(sql);
    }

    // Override to exclude soft-deleted entities by default
    public override async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = SqlQueryBuilder.For(TableName)
            .Where(IdColumnName, "Id")
            .WhereCustom($"{IsDeletedColumnName} = false")
            .Build();
        return await connection.QueryFirstOrDefaultAsync<TEntity>(sql, new { Id = id });
    }

    public override async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = SqlQueryBuilder.For(TableName)
            .WhereCustom($"{IsDeletedColumnName} = false")
            .Build();
        return await connection.QueryAsync<TEntity>(sql);
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
        var sql = SqlQueryBuilder.For(TableName)
            .Where(IdColumnName, "Id")
            .Where(TenantIdColumnName, "TenantId")
            .Build();
        return await connection.QueryFirstOrDefaultAsync<TEntity>(sql, new { Id = id, TenantId = tenantId });
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = SqlQueryBuilder.For(TableName)
            .Where(TenantIdColumnName, "TenantId")
            .Build();
        return await connection.QueryAsync<TEntity>(sql, new { TenantId = tenantId });
    }

    public virtual async Task<PagedResult<TEntity>> GetPagedByTenantAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        
        var offset = (page - 1) * pageSize;
        var countSql = SqlQueryBuilder.For(TableName)
            .Where(TenantIdColumnName, "TenantId")
            .Count()
            .Build();
        var dataSql = SqlQueryBuilder.For(TableName)
            .Where(TenantIdColumnName, "TenantId")
            .OrderBy(IdColumnName)
            .Limit(pageSize)
            .Offset(offset)
            .Build();

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
        var sql = SqlQueryBuilder.For(TableName)
            .Where(TenantIdColumnName, "TenantId")
            .Count()
            .Build();
        return await connection.ExecuteScalarAsync<int>(sql, new { TenantId = tenantId });
    }
}

