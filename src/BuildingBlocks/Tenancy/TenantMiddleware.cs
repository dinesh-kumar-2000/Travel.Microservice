using Microsoft.AspNetCore.Http;

namespace Tenancy;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantResolver resolver, TenantContext tenantContext)
    {
        var tenantId = await resolver.ResolveAsync(context);
        
        if (!string.IsNullOrEmpty(tenantId))
        {
            tenantContext.SetTenant(tenantId);
        }

        await _next(context);
    }
}

