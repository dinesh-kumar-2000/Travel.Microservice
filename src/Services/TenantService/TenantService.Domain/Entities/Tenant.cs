using SharedKernel.Models;

namespace TenantService.Domain.Entities;

public class Tenant : BaseEntity<string>
{
    public string Name { get; private set; } = string.Empty;
    public string Subdomain { get; private set; } = string.Empty;
    public string? CustomDomain { get; private set; }
    public string ContactEmail { get; private set; } = string.Empty;
    public string ContactPhone { get; private set; } = string.Empty;
    public TenantStatus Status { get; private set; } = TenantStatus.Active;
    public SubscriptionTier Tier { get; private set; } = SubscriptionTier.Basic;
    public TenantConfiguration Configuration { get; private set; } = TenantConfiguration.Default();
    public DateTime? SubscriptionExpiresAt { get; private set; }

    private Tenant() { }

    public static Tenant Create(string id, string name, string subdomain, string contactEmail, string contactPhone)
    {
        return new Tenant
        {
            Id = id,
            Name = name,
            Subdomain = subdomain.ToLowerInvariant(),
            ContactEmail = contactEmail,
            ContactPhone = contactPhone,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateConfiguration(TenantConfiguration configuration)
    {
        Configuration = configuration;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSubscriptionTier(SubscriptionTier tier, DateTime expiresAt)
    {
        Tier = tier;
        SubscriptionExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = TenantStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Suspend()
    {
        Status = TenantStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum TenantStatus
{
    Active,
    Suspended,
    Inactive
}

public enum SubscriptionTier
{
    Basic,
    Standard,
    Premium,
    Enterprise
}


