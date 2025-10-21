using Dapper;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Repositories;
using SharedKernel.Models;
using SharedKernel.Caching;
using System.Data;
using Npgsql;
using Tenancy;

namespace CatalogService.Infrastructure.Repositories;

public class PackageRepository : IPackageRepository
{
    private readonly string _connectionString;
    private readonly ITenantContext _tenantContext;
    private readonly ICacheService _cache;

    public PackageRepository(string connectionString, ITenantContext tenantContext, ICacheService cache)
    {
        _connectionString = connectionString;
        _tenantContext = tenantContext;
        _cache = cache;
    }

    private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    public async Task<Package?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"package:{_tenantContext.TenantId}:{id}";
        
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT * FROM packages 
                WHERE id = @Id AND tenant_id = @TenantId AND is_deleted = false";

            return await connection.QueryFirstOrDefaultAsync<Package>(sql, new 
            { 
                Id = id, 
                TenantId = _tenantContext.TenantId 
            });
        }, TimeSpan.FromMinutes(10), cancellationToken);
    }

    public async Task<IEnumerable<Package>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        const string sql = @"
            SELECT * FROM packages 
            WHERE tenant_id = @TenantId AND is_deleted = false
            ORDER BY created_at DESC";

        return await connection.QueryAsync<Package>(sql, new { TenantId = _tenantContext.TenantId });
    }

    public async Task<string> AddAsync(Package entity, CancellationToken cancellationToken = default)
    {
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
                @StartDate, @EndDate, @IsDeleted, @CreatedAt
            )";

        await connection.ExecuteAsync(sql, entity);
        
        // Invalidate cache
        await _cache.RemoveAsync($"package:{entity.TenantId}:{entity.Id}", cancellationToken);
        
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(Package entity, CancellationToken cancellationToken = default)
    {
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
        
        return rowsAffected > 0;
    }

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
        
        // Invalidate cache after reservation
        if (result)
        {
            await _cache.RemoveAsync($"package:{_tenantContext.TenantId}:{packageId}", cancellationToken);
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
        
        // Invalidate cache after release
        if (result)
        {
            await _cache.RemoveAsync($"package:{_tenantContext.TenantId}:{packageId}", cancellationToken);
        }
        
        return result;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        const string sql = @"
            UPDATE packages 
            SET is_deleted = true, 
                deleted_at = @DeletedAt 
            WHERE id = @Id AND tenant_id = @TenantId";

        var rowsAffected = await connection.ExecuteAsync(sql, new 
        { 
            Id = id, 
            TenantId = _tenantContext.TenantId,
            DeletedAt = DateTime.UtcNow 
        });
        
        // Invalidate cache
        await _cache.RemoveAsync($"package:{_tenantContext.TenantId}:{id}", cancellationToken);
        
        return rowsAffected > 0;
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
}

