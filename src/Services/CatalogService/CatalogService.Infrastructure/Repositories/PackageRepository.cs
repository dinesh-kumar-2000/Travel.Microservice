using Dapper;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Repositories;
using SharedKernel.Models;
using SharedKernel.Caching;
using SharedKernel.Data;
using Microsoft.Extensions.Logging;

namespace CatalogService.Infrastructure.Repositories;

/// <summary>
/// Repository for package operations
/// Inherits common CRUD operations from TenantBaseRepository
/// </summary>
public class PackageRepository : TenantBaseRepository<Package, string>, IPackageRepository
{
    private readonly ICacheService _cache;

    protected override string TableName => "packages";
    protected override string IdColumnName => "id";
    protected override string TenantIdColumnName => "tenant_id";

    public PackageRepository(IDapperContext context, ICacheService cache, ILogger<PackageRepository> logger) 
        : base(context, logger)
    {
        _cache = cache;
    }

    #region Overridden Methods

    public override async Task<string> AddAsync(Package entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        using var connection = CreateConnection();
        const string sql = @"
            INSERT INTO packages (
                id, tenant_id, name, description, destination, duration_days, 
                price, currency, max_capacity, available_slots, status, 
                start_date, end_date, is_deleted, created_at
            )
            VALUES (
                @Id, @TenantId, @Name, @Description, @Destination, @DurationDays,
                @Price, @Currency, @MaxCapacity, @AvailableSlots, @Status,
                @StartDate, @EndDate, false, @CreatedAt
            )";

        await connection.ExecuteAsync(sql, entity);
        
        // Invalidate cache
        await _cache.RemoveAsync($"package:{entity.TenantId}:{entity.Id}", cancellationToken);
        
        _logger.LogInformation("Package {PackageId} '{PackageName}' created for tenant {TenantId}", entity.Id, entity.Name, entity.TenantId);
        return entity.Id;
    }

    public override async Task<bool> UpdateAsync(Package entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        using var connection = CreateConnection();
        const string sql = @"
            UPDATE packages 
            SET name = @Name,
                description = @Description,
                destination = @Destination,
                duration_days = @DurationDays,
                price = @Price,
                currency = @Currency,
                max_capacity = @MaxCapacity,
                available_slots = @AvailableSlots,
                status = @Status,
                start_date = @StartDate,
                end_date = @EndDate,
                updated_at = CURRENT_TIMESTAMP,
                updated_by = @UpdatedBy
            WHERE id = @Id AND tenant_id = @TenantId";

        var rowsAffected = await connection.ExecuteAsync(sql, entity);
        
        // Invalidate cache
        await _cache.RemoveAsync($"package:{entity.TenantId}:{entity.Id}", cancellationToken);
        
        if (rowsAffected > 0)
        {
            _logger.LogInformation("Package {PackageId} updated", entity.Id);
        }
        
        return rowsAffected > 0;
    }

    #endregion

    #region Domain-Specific Methods

    public async Task<bool> ReserveSlotsAsync(string packageId, int quantity, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        
        // Use database function for atomic reservation with row locking
        const string sql = "SELECT fn_reserve_package_slots(@PackageId, @Quantity)";
        
        var result = await connection.ExecuteScalarAsync<bool>(sql, new 
        { 
            PackageId = packageId, 
            Quantity = quantity 
        });
        
        // Invalidate cache after reservation (use wildcard for tenant)
        if (result)
        {
            await _cache.RemoveByPrefixAsync($"package:", cancellationToken);
        }
        
        return result;
    }

    public async Task<bool> ReleaseSlotsAsync(string packageId, int quantity, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        
        // Use database function for atomic release
        const string sql = "SELECT fn_release_package_slots(@PackageId, @Quantity)";
        
        var result = await connection.ExecuteScalarAsync<bool>(sql, new 
        { 
            PackageId = packageId, 
            Quantity = quantity 
        });
        
        // Invalidate cache after release (use wildcard for tenant)
        if (result)
        {
            await _cache.RemoveByPrefixAsync($"package:", cancellationToken);
        }
        
        return result;
    }

    public async Task<PagedResult<Package>> SearchAsync(
        string tenantId, 
        string? destination, 
        decimal? minPrice, 
        decimal? maxPrice, 
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        
        var whereClauses = new List<string> { "tenant_id = @TenantId", "is_deleted = false", "status = 1" };
        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);
        parameters.Add("Offset", (pageNumber - 1) * pageSize);
        parameters.Add("PageSize", pageSize);

        if (!string.IsNullOrEmpty(destination))
        {
            whereClauses.Add("destination ILIKE @Destination");
            parameters.Add("Destination", $"%{destination}%");
        }

        if (minPrice.HasValue)
        {
            whereClauses.Add("price >= @MinPrice");
            parameters.Add("MinPrice", minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            whereClauses.Add("price <= @MaxPrice");
            parameters.Add("MaxPrice", maxPrice.Value);
        }

        var whereClause = string.Join(" AND ", whereClauses);

        var countSql = $"SELECT COUNT(*) FROM packages WHERE {whereClause}";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        var dataSql = $@"
            SELECT * FROM packages 
            WHERE {whereClause}
            ORDER BY created_at DESC
            OFFSET @Offset LIMIT @PageSize";

        var items = await connection.QueryAsync<Package>(dataSql, parameters);

        return new PagedResult<Package>(items.ToList(), totalCount, pageNumber, pageSize);
    }

    #endregion
}

