using Dapper;
using CatalogService.Domain.Entities;
using CatalogService.Application.Interfaces;
using SharedKernel.Data;
using Microsoft.Extensions.Logging;
using SharedKernel.Models;

namespace CatalogService.Infrastructure.Persistence.Repositories;

public class DestinationRepository : BaseRepository<Destination, string>, IDestinationRepository
{
    protected override string TableName => "destinations";
    protected override string IdColumnName => "id";

    public DestinationRepository(IDapperContext context, ILogger<DestinationRepository> logger) 
        : base(context, logger)
    {
    }

    public async Task<IEnumerable<Destination>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(searchTerm))
            return await GetAllAsync(cancellationToken);

        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT * FROM destinations 
            WHERE (name ILIKE @SearchTerm OR description ILIKE @SearchTerm OR city ILIKE @SearchTerm OR country ILIKE @SearchTerm)
            AND is_deleted = false
            ORDER BY name";

        var searchPattern = $"%{searchTerm}%";
        return await connection.QueryAsync<Destination>(sql, new { SearchTerm = searchPattern });
    }

    public async Task<IEnumerable<Destination>> GetByTypeAsync(int destinationType, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT * FROM destinations 
            WHERE destination_type = @DestinationType 
            AND is_deleted = false
            ORDER BY name";

        return await connection.QueryAsync<Destination>(sql, new { DestinationType = destinationType });
    }

    public async Task<IEnumerable<Destination>> GetByCountryAsync(string country, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(country))
            throw new ArgumentException("Country cannot be null or empty", nameof(country));

        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT * FROM destinations 
            WHERE country ILIKE @Country 
            AND is_deleted = false
            ORDER BY name";

        return await connection.QueryAsync<Destination>(sql, new { Country = $"%{country}%" });
    }

    public new async Task<PaginatedResult<Destination>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        
        const string countSql = "SELECT COUNT(*) FROM destinations WHERE is_deleted = false";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql);

        const string sql = @"
            SELECT * FROM destinations 
            WHERE is_deleted = false
            ORDER BY name
            LIMIT @PageSize OFFSET @Offset";

        var offset = (pageNumber - 1) * pageSize;
        var items = await connection.QueryAsync<Destination>(sql, new { PageSize = pageSize, Offset = offset });

        return new PaginatedResult<Destination>
        {
            Items = items.ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public override async Task<string> AddAsync(Destination entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        using var connection = CreateConnection();
        
        const string sql = @"
            INSERT INTO destinations (id, name, description, country, city, destination_type, latitude, longitude, is_active, created_at, created_by, tenant_id)
            VALUES (@Id, @Name, @Description, @Country, @City, @DestinationType, @Latitude, @Longitude, @IsActive, @CreatedAt, @CreatedBy, @TenantId)
            RETURNING id";

        var id = await connection.ExecuteScalarAsync<string>(sql, entity);
        return id;
    }

    public override async Task<bool> UpdateAsync(Destination entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        using var connection = CreateConnection();
        
        const string sql = @"
            UPDATE destinations 
            SET name = @Name, description = @Description, country = @Country, city = @City, 
                destination_type = @DestinationType, latitude = @Latitude, longitude = @Longitude, 
                is_active = @IsActive, updated_at = @UpdatedAt, updated_by = @UpdatedBy
            WHERE id = @Id AND is_deleted = false";

        var rowsAffected = await connection.ExecuteAsync(sql, entity);
        return rowsAffected > 0;
    }
}
