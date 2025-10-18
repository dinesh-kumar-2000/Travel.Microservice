using Dapper;
using TenantService.Domain.Entities;
using TenantService.Domain.Repositories;
using System.Data;
using Npgsql;
using System.Text.Json;

namespace TenantService.Infrastructure.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly string _connectionString;

    public TenantRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    public async Task<Tenant?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = "SELECT * FROM tenants WHERE id = @Id";
        var data = await connection.QueryFirstOrDefaultAsync<TenantData>(sql, new { Id = id });
        return data?.ToEntity();
    }

    public async Task<IEnumerable<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = "SELECT * FROM tenants ORDER BY created_at DESC";
        var data = await connection.QueryAsync<TenantData>(sql);
        return data.Select(d => d.ToEntity());
    }

    public async Task<string> AddAsync(Tenant entity, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = @"INSERT INTO tenants (id, name, subdomain, custom_domain, contact_email, contact_phone, 
                    status, tier, configuration, subscription_expires_at, created_at)
                    VALUES (@Id, @Name, @Subdomain, @CustomDomain, @ContactEmail, @ContactPhone, 
                    @Status, @Tier, @Configuration::jsonb, @SubscriptionExpiresAt, @CreatedAt)";
        
        await connection.ExecuteAsync(sql, TenantData.FromEntity(entity));
        return entity.Id;
    }

    public async Task UpdateAsync(Tenant entity, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = @"UPDATE tenants SET name = @Name, custom_domain = @CustomDomain, 
                    contact_email = @ContactEmail, contact_phone = @ContactPhone,
                    status = @Status, tier = @Tier, configuration = @Configuration::jsonb, 
                    subscription_expires_at = @SubscriptionExpiresAt, updated_at = @UpdatedAt
                    WHERE id = @Id";
        
        await connection.ExecuteAsync(sql, TenantData.FromEntity(entity));
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        await connection.ExecuteAsync("DELETE FROM tenants WHERE id = @Id", new { Id = id });
    }

    public async Task<Tenant?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = "SELECT * FROM tenants WHERE subdomain = @Subdomain";
        var data = await connection.QueryFirstOrDefaultAsync<TenantData>(sql, new { Subdomain = subdomain });
        return data?.ToEntity();
    }

    public async Task<bool> SubdomainExistsAsync(string subdomain, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM tenants WHERE subdomain = @Subdomain", new { Subdomain = subdomain });
        return count > 0;
    }

    public async Task<IEnumerable<Tenant>> GetActiveTenantsAsync(CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = "SELECT * FROM tenants WHERE status = 0 ORDER BY name";  // 0 = Active status
        var data = await connection.QueryAsync<TenantData>(sql);
        return data.Select(d => d.ToEntity());
    }

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
}

