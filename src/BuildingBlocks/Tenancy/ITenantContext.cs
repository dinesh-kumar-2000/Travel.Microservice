namespace Tenancy;

public interface ITenantContext
{
    string? TenantId { get; }
    bool IsResolved { get; }
}

public class TenantContext : ITenantContext
{
    public string? TenantId { get; private set; }
    public bool IsResolved => !string.IsNullOrEmpty(TenantId);

    public void SetTenant(string tenantId)
    {
        TenantId = tenantId;
    }
}

