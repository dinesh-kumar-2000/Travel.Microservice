using Dapper;
using TenantService.Domain.Entities;
using TenantService.Domain.Interfaces;
using SharedKernel.Data;
using Microsoft.Extensions.Logging;

namespace TenantService.Infrastructure.Repositories;

public class TenantConfigurationRepository : BaseRepository<TenantConfiguration, string>, ITenantConfigurationRepository
{
    protected override string TableName => "tenant_configurations";
    protected override string IdColumnName => "id";

    public TenantConfigurationRepository(IDapperContext context, ILogger<TenantConfigurationRepository> logger) 
        : base(context, logger)
    {
    }

    public async Task<TenantConfiguration?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));

        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT * FROM tenant_configurations 
            WHERE tenant_id = @TenantId 
            AND is_deleted = false";

        return await connection.QueryFirstOrDefaultAsync<TenantConfiguration>(sql, new { TenantId = tenantId });
    }

    public async Task<bool> ExistsForTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));

        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT COUNT(1) 
            FROM tenant_configurations 
            WHERE tenant_id = @TenantId 
            AND is_deleted = false";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { TenantId = tenantId });
        return count > 0;
    }

    public override async Task<string> AddAsync(TenantConfiguration entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        using var connection = CreateConnection();
        
        const string sql = @"
            INSERT INTO tenant_configurations (id, tenant_id, theme_settings, feature_flags, custom_settings, created_at, created_by, tenant_id)
            VALUES (@Id, @TenantId, @ThemeSettings, @FeatureFlags, @CustomSettings, @CreatedAt, @CreatedBy, @TenantId)
            RETURNING id";

        var id = await connection.ExecuteScalarAsync<string>(sql, entity);
        return id;
    }

    public override async Task<bool> UpdateAsync(TenantConfiguration entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        using var connection = CreateConnection();
        
        const string sql = @"
            UPDATE tenant_configurations 
            SET tenant_id = @TenantId, theme_settings = @ThemeSettings, feature_flags = @FeatureFlags, 
                custom_settings = @CustomSettings, updated_at = @UpdatedAt, updated_by = @UpdatedBy
            WHERE id = @Id AND is_deleted = false";

        var rowsAffected = await connection.ExecuteAsync(sql, entity);
        return rowsAffected > 0;
    }

}
