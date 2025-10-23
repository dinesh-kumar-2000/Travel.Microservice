using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Tenancy;

namespace IdentityService.API.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        // Extract tenant information from various sources
        var tenantId = ExtractTenantId(context);
        
        if (!string.IsNullOrEmpty(tenantId))
        {
            if (tenantContext is TenantContext contextImpl)
            {
                contextImpl.SetTenant(tenantId);
                _logger.LogInformation("Tenant context set to: {TenantId}", tenantId);
            }
        }
        else
        {
            _logger.LogWarning("No tenant ID found in request");
        }

        await _next(context);
    }

    private static string? ExtractTenantId(HttpContext context)
    {
        // Try to get tenant ID from various sources in order of preference
        
        // 1. From custom header
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerValue))
        {
            return headerValue.FirstOrDefault();
        }

        // 2. From query parameter
        if (context.Request.Query.TryGetValue("tenantId", out var queryValue))
        {
            return queryValue.FirstOrDefault();
        }

        // 3. From subdomain
        var host = context.Request.Host.Host;
        if (host.Contains('.'))
        {
            var subdomain = host.Split('.')[0];
            if (subdomain != "www" && subdomain != "api")
            {
                return subdomain;
            }
        }

        // 4. From JWT token claims
        var tenantClaim = context.User?.FindFirst("tenant_id");
        if (tenantClaim != null)
        {
            return tenantClaim.Value;
        }

        return null;
    }
}
