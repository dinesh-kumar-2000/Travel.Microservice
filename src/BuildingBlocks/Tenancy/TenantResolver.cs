using Microsoft.AspNetCore.Http;
using SharedKernel.Utilities;

namespace Tenancy;

public interface ITenantResolver
{
    Task<string?> ResolveAsync(HttpContext context);
}

public class TenantResolver : ITenantResolver
{
    public Task<string?> ResolveAsync(HttpContext context)
    {
        // Try JWT claim first
        var tenantId = context.User.GetTenantId();
        
        if (!string.IsNullOrEmpty(tenantId))
            return Task.FromResult<string?>(tenantId);

        // Try header
        tenantId = context.GetTenantIdFromHeader();
        if (!string.IsNullOrEmpty(tenantId))
            return Task.FromResult<string?>(tenantId);

        // Try subdomain (e.g., tenant1.travelportal.com)
        tenantId = context.GetTenantIdFromSubdomain();
        if (!string.IsNullOrEmpty(tenantId))
            return Task.FromResult<string?>(tenantId);

        return Task.FromResult<string?>(null);
    }
}

