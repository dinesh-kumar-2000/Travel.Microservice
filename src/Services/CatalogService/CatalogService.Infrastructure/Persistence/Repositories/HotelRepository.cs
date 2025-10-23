using Dapper;
using CatalogService.Domain.Entities;
using CatalogService.Application.Interfaces;
using SharedKernel.Data;
using Microsoft.Extensions.Logging;

namespace CatalogService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for hotel operations
/// Inherits common CRUD operations from TenantBaseRepository
/// </summary>
public class HotelRepository : TenantBaseRepository<Hotel, string>, IHotelRepository
{
    protected override string TableName => "hotels";
    protected override string IdColumnName => "id";
    protected override string TenantIdColumnName => "tenant_id";

    public HotelRepository(IDapperContext context, ILogger<HotelRepository> logger) 
        : base(context, logger)
    {
    }

    #region Overridden Methods

    public override async Task<string> AddAsync(Hotel entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        using var connection = CreateConnection();
        
        const string sql = @"
            INSERT INTO hotels (
                id, tenant_id, name, description, location, address, city, country,
                star_rating, price_per_night, currency, total_rooms, available_rooms,
                status, amenities, images, latitude, longitude, contact_email, contact_phone,
                created_at, is_deleted
            )
            VALUES (
                @Id, @TenantId, @Name, @Description, @Location, @Address, @City, @Country,
                @StarRating, @PricePerNight, @Currency, @TotalRooms, @AvailableRooms,
                @Status, @Amenities::jsonb, @Images::jsonb, @Latitude, @Longitude, @ContactEmail, @ContactPhone,
                @CreatedAt, false
            )";
        
        await connection.ExecuteAsync(sql, entity);
        _logger.LogInformation("Hotel {HotelId} '{HotelName}' created for tenant {TenantId}", entity.Id, entity.Name, entity.TenantId);
        return entity.Id;
    }

    public override async Task<bool> UpdateAsync(Hotel entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        using var connection = CreateConnection();
        
        const string sql = @"
            UPDATE hotels SET
                name = @Name,
                description = @Description,
                price_per_night = @PricePerNight,
                amenities = @Amenities::jsonb,
                images = @Images::jsonb,
                status = @Status,
                updated_at = @UpdatedAt
            WHERE id = @Id AND tenant_id = @TenantId";
        
        var rowsAffected = await connection.ExecuteAsync(sql, entity);
        
        if (rowsAffected > 0)
        {
            _logger.LogInformation("Hotel {HotelId} updated", entity.Id);
        }

        return rowsAffected > 0;
    }

    public override async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentNullException(nameof(id));

        using var connection = CreateConnection();
        
        const string sql = @"
            UPDATE hotels 
            SET is_deleted = true,
                deleted_at = @DeletedAt
            WHERE id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new 
        { 
            Id = id,
            DeletedAt = DateTime.UtcNow 
        });

        if (rowsAffected > 0)
        {
            _logger.LogInformation("Hotel {HotelId} deleted (soft delete)", id);
        }

        return rowsAffected > 0;
    }

    #endregion

    #region Domain-Specific Methods

    public async Task<(IEnumerable<Hotel> Hotels, int TotalCount)> SearchHotelsAsync(
        string tenantId,
        string? city = null,
        string? country = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int? starRating = null,
        string[]? amenities = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(tenantId))
            throw new ArgumentNullException(nameof(tenantId));
        if (page < 1)
            throw new ArgumentException("Page must be greater than 0", nameof(page));
        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentException("PageSize must be between 1 and 100", nameof(pageSize));

        using var connection = CreateConnection();
        
        var whereClauses = new List<string> { "tenant_id = @TenantId", "is_deleted = false", "status = @Status" };
        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);
        parameters.Add("Status", (int)HotelStatus.Active);
        
        if (!string.IsNullOrEmpty(city))
        {
            whereClauses.Add("LOWER(city) = LOWER(@City)");
            parameters.Add("City", city);
        }
        
        if (!string.IsNullOrEmpty(country))
        {
            whereClauses.Add("LOWER(country) = LOWER(@Country)");
            parameters.Add("Country", country);
        }
        
        if (minPrice.HasValue)
        {
            whereClauses.Add("price_per_night >= @MinPrice");
            parameters.Add("MinPrice", minPrice.Value);
        }
        
        if (maxPrice.HasValue)
        {
            whereClauses.Add("price_per_night <= @MaxPrice");
            parameters.Add("MaxPrice", maxPrice.Value);
        }
        
        if (starRating.HasValue)
        {
            whereClauses.Add("star_rating = @StarRating");
            parameters.Add("StarRating", starRating.Value);
        }
        
        var whereClause = string.Join(" AND ", whereClauses);
        
        // Get total count
        var countSql = $"SELECT COUNT(*) FROM hotels WHERE {whereClause}";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        
        // Get paged data
        var offset = (page - 1) * pageSize;
        var dataSql = $@"
            SELECT * FROM hotels 
            WHERE {whereClause}
            ORDER BY created_at DESC 
            LIMIT @PageSize OFFSET @Offset";
        
        parameters.Add("PageSize", pageSize);
        parameters.Add("Offset", offset);
        
        var hotels = await connection.QueryAsync<Hotel>(dataSql, parameters);
        
        _logger.LogDebug("Retrieved {Count} hotels for tenant {TenantId} matching search criteria", hotels.Count(), tenantId);
        
        return (hotels, totalCount);
    }

    public async Task<IEnumerable<Hotel>> GetByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(tenantId))
            throw new ArgumentNullException(nameof(tenantId));

        // Use inherited method from TenantBaseRepository
        return await GetAllByTenantAsync(Guid.Parse(tenantId), cancellationToken);
    }

    #endregion
}
