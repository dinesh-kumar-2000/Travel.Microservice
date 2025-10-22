using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using SharedKernel.Constants;
using System.Security.Claims;

namespace SharedKernel.Utilities;

/// <summary>
/// Extension methods for HttpContext to extract common values
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Gets the correlation ID from request headers or generates a new one
    /// </summary>
    public static string GetOrGenerateCorrelationId(this HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(ApplicationConstants.Headers.CorrelationId, out StringValues correlationId))
        {
            return correlationId.ToString();
        }

        return DefaultProviders.IdGenerator.Generate();
    }

    /// <summary>
    /// Gets the tenant ID from request headers
    /// </summary>
    public static string? GetTenantIdFromHeader(this HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(ApplicationConstants.Headers.TenantId, out var headerValue))
        {
            var tenantId = headerValue.ToString();
            if (!string.IsNullOrEmpty(tenantId))
                return tenantId;
        }

        return null;
    }

    /// <summary>
    /// Gets the tenant ID from subdomain (e.g., tenant1.domain.com -> tenant1)
    /// </summary>
    public static string? GetTenantIdFromSubdomain(this HttpContext context)
    {
        var host = context.Request.Host.Host;
        var parts = host.Split('.');
        if (parts.Length > 2)
        {
            return parts[0];
        }

        return null;
    }
}

/// <summary>
/// Extension methods for ClaimsPrincipal to extract common claim values
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Gets the user ID from claims
    /// </summary>
    public static string? GetUserId(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    /// <summary>
    /// Gets the email from claims
    /// </summary>
    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Email);
    }

    /// <summary>
    /// Gets the tenant ID from claims
    /// </summary>
    public static string? GetTenantId(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ApplicationConstants.Claims.TenantId);
    }

    /// <summary>
    /// Gets all roles from claims
    /// </summary>
    public static IEnumerable<string> GetRoles(this ClaimsPrincipal principal)
    {
        return principal.FindAll(ClaimTypes.Role).Select(c => c.Value);
    }
}

