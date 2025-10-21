using Dapper;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Repositories;
using CatalogService.Infrastructure.Data;
using Tenancy;

namespace CatalogService.Infrastructure.Repositories;

public class TourRepository : ITourRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantContext _tenantContext;

    public TourRepository(IDbConnectionFactory connectionFactory, ITenantContext tenantContext)
    {
        _connectionFactory = connectionFactory;
        _tenantContext = tenantContext;
    }

    public async Task<Tour?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   name AS Name,
                   description AS Description,
                   destination AS Destination,
                   locations AS Locations,
                   duration_days AS DurationDays,
                   price AS Price,
                   currency AS Currency,
                   max_group_size AS MaxGroupSize,
                   available_spots AS AvailableSpots,
                   status AS Status,
                   start_date AS StartDate,
                   end_date AS EndDate,
                   inclusions AS Inclusions,
                   exclusions AS Exclusions,
                   images AS Images,
                   difficulty AS Difficulty,
                   languages AS Languages,
                   min_age AS MinAge,
                   meeting_point AS MeetingPoint,
                   guide_info AS GuideInfo,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM tours
            WHERE id = @Id AND tenant_id = @TenantId AND is_deleted = false";

        return await connection.QueryFirstOrDefaultAsync<Tour>(sql, new 
        { 
            Id = id, 
            TenantId = _tenantContext.TenantId 
        });
    }

    public async Task<IEnumerable<Tour>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   name AS Name,
                   description AS Description,
                   destination AS Destination,
                   locations AS Locations,
                   duration_days AS DurationDays,
                   price AS Price,
                   currency AS Currency,
                   max_group_size AS MaxGroupSize,
                   available_spots AS AvailableSpots,
                   status AS Status,
                   start_date AS StartDate,
                   end_date AS EndDate,
                   inclusions AS Inclusions,
                   exclusions AS Exclusions,
                   images AS Images,
                   difficulty AS Difficulty,
                   languages AS Languages,
                   min_age AS MinAge,
                   meeting_point AS MeetingPoint,
                   guide_info AS GuideInfo,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM tours
            WHERE tenant_id = @TenantId AND is_deleted = false
            ORDER BY start_date DESC";

        return await connection.QueryAsync<Tour>(sql, new { TenantId = _tenantContext.TenantId });
    }

    public async Task<string> AddAsync(Tour entity, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO tours (
                id, tenant_id, name, description, destination, locations, duration_days,
                price, currency, max_group_size, available_spots, status, start_date, end_date,
                inclusions, exclusions, images, difficulty, languages, min_age,
                meeting_point, guide_info, created_at, is_deleted
            )
            VALUES (
                @Id, @TenantId, @Name, @Description, @Destination, @Locations::jsonb, @DurationDays,
                @Price, @Currency, @MaxGroupSize, @AvailableSpots, @Status, @StartDate, @EndDate,
                @Inclusions::jsonb, @Exclusions::jsonb, @Images::jsonb, @Difficulty, @Languages::jsonb, @MinAge,
                @MeetingPoint, @GuideInfo, @CreatedAt, false
            )";
        
        await connection.ExecuteAsync(sql, entity);
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(Tour entity, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE tours SET
                name = @Name,
                description = @Description,
                price = @Price,
                inclusions = @Inclusions::jsonb,
                exclusions = @Exclusions::jsonb,
                images = @Images::jsonb,
                status = @Status,
                updated_at = @UpdatedAt
            WHERE id = @Id AND tenant_id = @TenantId";
        
        var rowsAffected = await connection.ExecuteAsync(sql, entity);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE tours 
            SET is_deleted = true,
                deleted_at = @DeletedAt
            WHERE id = @Id AND tenant_id = @TenantId";

        var rowsAffected = await connection.ExecuteAsync(sql, new 
        { 
            Id = id, 
            TenantId = _tenantContext.TenantId, 
            DeletedAt = DateTime.UtcNow 
        });

        return rowsAffected > 0;
    }

    public async Task<(IEnumerable<Tour> Tours, int TotalCount)> SearchToursAsync(
        string tenantId,
        string? destination = null,
        int? minDuration = null,
        int? maxDuration = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? difficulty = null,
        DateTime? startDate = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var whereClauses = new List<string> { "tenant_id = @TenantId", "is_deleted = false", "status = @Status" };
        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);
        parameters.Add("Status", (int)TourStatus.Active);
        
        if (!string.IsNullOrEmpty(destination))
        {
            whereClauses.Add("LOWER(destination) LIKE LOWER(@Destination)");
            parameters.Add("Destination", $"%{destination}%");
        }
        
        if (minDuration.HasValue)
        {
            whereClauses.Add("duration_days >= @MinDuration");
            parameters.Add("MinDuration", minDuration.Value);
        }
        
        if (maxDuration.HasValue)
        {
            whereClauses.Add("duration_days <= @MaxDuration");
            parameters.Add("MaxDuration", maxDuration.Value);
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
        
        if (!string.IsNullOrEmpty(difficulty) && Enum.TryParse<DifficultyLevel>(difficulty, true, out var difficultyEnum))
        {
            whereClauses.Add("difficulty = @Difficulty");
            parameters.Add("Difficulty", (int)difficultyEnum);
        }
        
        if (startDate.HasValue)
        {
            whereClauses.Add("start_date >= @StartDate");
            parameters.Add("StartDate", startDate.Value.Date);
        }
        
        var whereClause = string.Join(" AND ", whereClauses);
        
        // Get total count
        var countSql = $"SELECT COUNT(*) FROM tours WHERE {whereClause}";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        
        // Get paged data
        var offset = (page - 1) * pageSize;
        var dataSql = $@"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   name AS Name,
                   description AS Description,
                   destination AS Destination,
                   locations AS Locations,
                   duration_days AS DurationDays,
                   price AS Price,
                   currency AS Currency,
                   max_group_size AS MaxGroupSize,
                   available_spots AS AvailableSpots,
                   status AS Status,
                   start_date AS StartDate,
                   end_date AS EndDate,
                   inclusions AS Inclusions,
                   exclusions AS Exclusions,
                   images AS Images,
                   difficulty AS Difficulty,
                   languages AS Languages,
                   min_age AS MinAge,
                   meeting_point AS MeetingPoint,
                   guide_info AS GuideInfo,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM tours 
            WHERE {whereClause}
            ORDER BY start_date ASC 
            LIMIT @PageSize OFFSET @Offset";
        
        parameters.Add("PageSize", pageSize);
        parameters.Add("Offset", offset);
        
        var tours = await connection.QueryAsync<Tour>(dataSql, parameters);
        
        return (tours, totalCount);
    }

    public async Task<IEnumerable<Tour>> GetByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   name AS Name,
                   description AS Description,
                   destination AS Destination,
                   locations AS Locations,
                   duration_days AS DurationDays,
                   price AS Price,
                   currency AS Currency,
                   max_group_size AS MaxGroupSize,
                   available_spots AS AvailableSpots,
                   status AS Status,
                   start_date AS StartDate,
                   end_date AS EndDate,
                   inclusions AS Inclusions,
                   exclusions AS Exclusions,
                   images AS Images,
                   difficulty AS Difficulty,
                   languages AS Languages,
                   min_age AS MinAge,
                   meeting_point AS MeetingPoint,
                   guide_info AS GuideInfo,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM tours
            WHERE tenant_id = @TenantId AND is_deleted = false
            ORDER BY start_date DESC";

        return await connection.QueryAsync<Tour>(sql, new { TenantId = tenantId });
    }
}

