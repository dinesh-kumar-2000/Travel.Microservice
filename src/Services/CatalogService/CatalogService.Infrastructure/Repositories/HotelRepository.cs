using Dapper;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Repositories;
using CatalogService.Infrastructure.Data;
using Tenancy;

namespace CatalogService.Infrastructure.Repositories;

public class HotelRepository : IHotelRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantContext _tenantContext;

    public HotelRepository(IDbConnectionFactory connectionFactory, ITenantContext tenantContext)
    {
        _connectionFactory = connectionFactory;
        _tenantContext = tenantContext;
    }

    public async Task<Hotel?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   name AS Name,
                   description AS Description,
                   location AS Location,
                   address AS Address,
                   city AS City,
                   country AS Country,
                   star_rating AS StarRating,
                   price_per_night AS PricePerNight,
                   currency AS Currency,
                   total_rooms AS TotalRooms,
                   available_rooms AS AvailableRooms,
                   status AS Status,
                   amenities AS Amenities,
                   images AS Images,
                   latitude AS Latitude,
                   longitude AS Longitude,
                   contact_email AS ContactEmail,
                   contact_phone AS ContactPhone,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM hotels
            WHERE id = @Id AND tenant_id = @TenantId AND is_deleted = false";

        return await connection.QueryFirstOrDefaultAsync<Hotel>(sql, new 
        { 
            Id = id, 
            TenantId = _tenantContext.TenantId 
        });
    }

    public async Task<IEnumerable<Hotel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   name AS Name,
                   description AS Description,
                   location AS Location,
                   address AS Address,
                   city AS City,
                   country AS Country,
                   star_rating AS StarRating,
                   price_per_night AS PricePerNight,
                   currency AS Currency,
                   total_rooms AS TotalRooms,
                   available_rooms AS AvailableRooms,
                   status AS Status,
                   amenities AS Amenities,
                   images AS Images,
                   latitude AS Latitude,
                   longitude AS Longitude,
                   contact_email AS ContactEmail,
                   contact_phone AS ContactPhone,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM hotels
            WHERE tenant_id = @TenantId AND is_deleted = false
            ORDER BY created_at DESC";

        return await connection.QueryAsync<Hotel>(sql, new { TenantId = _tenantContext.TenantId });
    }

    public async Task<string> AddAsync(Hotel entity, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
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
        return entity.Id;
    }

    public async Task UpdateAsync(Hotel entity, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
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
        
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE hotels 
            SET is_deleted = true,
                deleted_at = @DeletedAt
            WHERE id = @Id AND tenant_id = @TenantId";

        await connection.ExecuteAsync(sql, new 
        { 
            Id = id, 
            TenantId = _tenantContext.TenantId, 
            DeletedAt = DateTime.UtcNow 
        });
    }

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
        using var connection = _connectionFactory.CreateConnection();
        
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
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   name AS Name,
                   description AS Description,
                   location AS Location,
                   address AS Address,
                   city AS City,
                   country AS Country,
                   star_rating AS StarRating,
                   price_per_night AS PricePerNight,
                   currency AS Currency,
                   total_rooms AS TotalRooms,
                   available_rooms AS AvailableRooms,
                   status AS Status,
                   amenities AS Amenities,
                   images AS Images,
                   latitude AS Latitude,
                   longitude AS Longitude,
                   contact_email AS ContactEmail,
                   contact_phone AS ContactPhone,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM hotels 
            WHERE {whereClause}
            ORDER BY created_at DESC 
            LIMIT @PageSize OFFSET @Offset";
        
        parameters.Add("PageSize", pageSize);
        parameters.Add("Offset", offset);
        
        var hotels = await connection.QueryAsync<Hotel>(dataSql, parameters);
        
        return (hotels, totalCount);
    }

    public async Task<IEnumerable<Hotel>> GetByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   name AS Name,
                   description AS Description,
                   location AS Location,
                   address AS Address,
                   city AS City,
                   country AS Country,
                   star_rating AS StarRating,
                   price_per_night AS PricePerNight,
                   currency AS Currency,
                   total_rooms AS TotalRooms,
                   available_rooms AS AvailableRooms,
                   status AS Status,
                   amenities AS Amenities,
                   images AS Images,
                   latitude AS Latitude,
                   longitude AS Longitude,
                   contact_email AS ContactEmail,
                   contact_phone AS ContactPhone,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM hotels
            WHERE tenant_id = @TenantId AND is_deleted = false
            ORDER BY created_at DESC";

        return await connection.QueryAsync<Hotel>(sql, new { TenantId = tenantId });
    }
}

