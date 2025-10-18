using Microsoft.AspNetCore.Http;
using System.Security.Claims;

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
        var tenantId = context.User.FindFirstValue("tenant_id");
        
        if (!string.IsNullOrEmpty(tenantId))
            return Task.FromResult<string?>(tenantId);

        // Try header
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerValue))
        {
            tenantId = headerValue.ToString();
            if (!string.IsNullOrEmpty(tenantId))
                return Task.FromResult<string?>(tenantId);
        }

        // Try subdomain (e.g., tenant1.travelportal.com)
        var host = context.Request.Host.Host;
        var parts = host.Split('.');
        if (parts.Length > 2)
        {
            tenantId = parts[0];
            return Task.FromResult<string?>(tenantId);
        }

        return Task.FromResult<string?>(null);
    }
}

