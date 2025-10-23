using Dapper;
using TenantService.Domain.Entities;
using TenantService.Application.Interfaces;
using SharedKernel.Data;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace TenantService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for tenant operations
/// Inherits common CRUD operations from BaseRepository
/// Note: Tenants themselves are not tenant-scoped, so we use BaseRepository instead of TenantBaseRepository
/// </summary>
public class TenantRepository : BaseRepository<Tenant, string>, ITenantRepository
{
    protected override string TableName => "tenants";
    protected override string IdColumnName => "id";

    public TenantRepository(IDapperContext context, ILogger<TenantRepository> logger) 
        : base(context, logger)
    {
    }

    #region Overridden Methods

    public override async Task<Tenant?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentNullException(nameof(id));

        using var connection = CreateConnection();
        var sql = "SELECT * FROM tenants WHERE id = @Id";
        var data = await connection.QueryFirstOrDefaultAsync<TenantData>(sql, new { Id = id });
        return data?.ToEntity();
    }

    public override async Task<IEnumerable<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = "SELECT * FROM tenants ORDER BY created_at DESC";
        var data = await connection.QueryAsync<TenantData>(sql);
        return data.Select(d => d.ToEntity());
    }

    public override async Task<string> AddAsync(Tenant entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        using var connection = CreateConnection();
        var sql = @"INSERT INTO tenants (id, name, subdomain, custom_domain, contact_email, contact_phone, 
                    status, tier, configuration, subscription_expires_at, created_at)
                    VALUES (@Id, @Name, @Subdomain, @CustomDomain, @ContactEmail, @ContactPhone, 
                    @Status, @Tier, @Configuration::jsonb, @SubscriptionExpiresAt, @CreatedAt)";
        
        await connection.ExecuteAsync(sql, TenantData.FromEntity(entity));
        _logger.LogInformation("Tenant {TenantId} '{TenantName}' created with subdomain '{Subdomain}'", 
            entity.Id, entity.Name, entity.Subdomain);
        return entity.Id;
    }

    public override async Task<bool> UpdateAsync(Tenant entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        using var connection = CreateConnection();
        var sql = @"UPDATE tenants SET name = @Name, custom_domain = @CustomDomain, 
                    contact_email = @ContactEmail, contact_phone = @ContactPhone,
                    status = @Status, tier = @Tier, configuration = @Configuration::jsonb, 
                    subscription_expires_at = @SubscriptionExpiresAt, updated_at = @UpdatedAt
                    WHERE id = @Id";
        
        var rowsAffected = await connection.ExecuteAsync(sql, TenantData.FromEntity(entity));
        
        if (rowsAffected > 0)
        {
            _logger.LogInformation("Tenant {TenantId} updated", entity.Id);
        }

        return rowsAffected > 0;
    }

    public override async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentNullException(nameof(id));

        using var connection = CreateConnection();
        var rowsAffected = await connection.ExecuteAsync("DELETE FROM tenants WHERE id = @Id", new { Id = id });
        
        if (rowsAffected > 0)
        {
            _logger.LogWarning("Tenant {TenantId} permanently deleted", id);
        }

        return rowsAffected > 0;
    }

    #endregion

    #region Domain-Specific Methods

    public async Task<Tenant?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(subdomain))
            throw new ArgumentNullException(nameof(subdomain));

        using var connection = CreateConnection();
        var sql = "SELECT * FROM tenants WHERE subdomain = @Subdomain";
        var data = await connection.QueryFirstOrDefaultAsync<TenantData>(sql, new { Subdomain = subdomain });
        return data?.ToEntity();
    }

    public async Task<bool> SubdomainExistsAsync(string subdomain, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(subdomain))
            throw new ArgumentNullException(nameof(subdomain));

        return await ExistsAsync(subdomain, cancellationToken);
    }

    public async Task<IEnumerable<Tenant>> GetActiveTenantsAsync(CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = "SELECT * FROM tenants WHERE status = 0 ORDER BY name";  // 0 = Active status
        var data = await connection.QueryAsync<TenantData>(sql);
        return data.Select(d => d.ToEntity());
    }

    public async Task<(IEnumerable<Tenant> Tenants, int TotalCount)> GetPagedTenantsAsync(
        int page, 
        int pageSize, 
        string? status = null, 
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            throw new ArgumentException("Page must be greater than 0", nameof(page));
        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentException("PageSize must be between 1 and 100", nameof(pageSize));

        using var connection = CreateConnection();
        
        var whereClauses = new List<string>();
        var parameters = new DynamicParameters();
        
        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<TenantStatus>(status, true, out var statusEnum))
            {
                whereClauses.Add("status = @Status");
                parameters.Add("Status", (int)statusEnum);
            }
        }
        
        var whereClause = whereClauses.Any() ? $"WHERE {string.Join(" AND ", whereClauses)}" : "";
        
        // Get total count
        var countSql = $"SELECT COUNT(*) FROM tenants {whereClause}";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        
        // Get paged data
        var offset = (page - 1) * pageSize;
        var dataSql = $@"SELECT * FROM tenants {whereClause} 
                        ORDER BY created_at DESC 
                        LIMIT @PageSize OFFSET @Offset";
        
        parameters.Add("PageSize", pageSize);
        parameters.Add("Offset", offset);
        
        var data = await connection.QueryAsync<TenantData>(dataSql, parameters);
        var tenants = data.Select(d => d.ToEntity());
        
        _logger.LogDebug("Retrieved {Count} tenants (page {Page})", tenants.Count(), page);
        
        return (tenants, totalCount);
    }

    public async Task<Dictionary<string, int>> GetTenantStatisticsAsync(CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        
        var stats = new Dictionary<string, int>();
        
        // Total tenants
        stats["TotalTenants"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM tenants");
        
        // By status
        stats["ActiveTenants"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM tenants WHERE status = 0");
        stats["SuspendedTenants"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM tenants WHERE status = 1");
        stats["InactiveTenants"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM tenants WHERE status = 2");
        
        // By tier
        stats["BasicTier"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM tenants WHERE tier = 0");
        stats["StandardTier"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM tenants WHERE tier = 1");
        stats["PremiumTier"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM tenants WHERE tier = 2");
        stats["EnterpriseTier"] = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM tenants WHERE tier = 3");
        
        _logger.LogDebug("Retrieved tenant statistics: {TotalCount} total tenants", stats["TotalTenants"]);
        
        return stats;
    }

    #endregion

    #region Helper Classes

    private class TenantData
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Subdomain { get; set; } = string.Empty;
        public string? CustomDomain { get; set; }
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public int Status { get; set; }
        public int Tier { get; set; }
        public string Configuration { get; set; } = "{}";
        public DateTime? SubscriptionExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Tenant ToEntity()
        {
            var tenant = Tenant.Create(Id, Name, Subdomain, ContactEmail, ContactPhone);
            var config = JsonSerializer.Deserialize<TenantConfiguration>(Configuration) ?? new TenantConfiguration();
            typeof(Tenant).GetProperty("Configuration")!.SetValue(tenant, config);
            typeof(Tenant).GetProperty("Status")!.SetValue(tenant, (TenantStatus)Status);
            typeof(Tenant).GetProperty("Tier")!.SetValue(tenant, (SubscriptionTier)Tier);
            typeof(Tenant).GetProperty("CustomDomain")!.SetValue(tenant, CustomDomain);
            typeof(Tenant).GetProperty("SubscriptionExpiresAt")!.SetValue(tenant, SubscriptionExpiresAt);
            return tenant;
        }

        public static TenantData FromEntity(Tenant entity)
        {
            return new TenantData
            {
                Id = entity.Id,
                Name = entity.Name,
                Subdomain = entity.Subdomain,
                CustomDomain = entity.CustomDomain,
                ContactEmail = entity.ContactEmail,
                ContactPhone = entity.ContactPhone,
                Status = (int)entity.Status,
                Tier = (int)entity.Tier,
                Configuration = JsonSerializer.Serialize(entity.Configuration),
                SubscriptionExpiresAt = entity.SubscriptionExpiresAt,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }
    }

    #endregion
}
