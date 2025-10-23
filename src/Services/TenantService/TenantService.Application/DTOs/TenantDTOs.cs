using System.Text.Json.Serialization;

namespace TenantService.Application.DTOs;

public record CreateTenantRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("subdomain")] string Subdomain,
    [property: JsonPropertyName("contactEmail")] string ContactEmail,
    [property: JsonPropertyName("contactPhone")] string ContactPhone
);

public record TenantDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("subdomain")] string Subdomain,
    [property: JsonPropertyName("contactEmail")] string ContactEmail,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("tier")] string Tier,
    [property: JsonPropertyName("logo")] string? Logo,
    [property: JsonPropertyName("primaryColor")] string? PrimaryColor,
    [property: JsonPropertyName("secondaryColor")] string? SecondaryColor,
    [property: JsonPropertyName("isActive")] bool IsActive,
    [property: JsonPropertyName("settings")] TenantSettings? Settings = null
);

public record TenantSettings(
    [property: JsonPropertyName("theme")] string? Theme = null,
    [property: JsonPropertyName("primaryColor")] string? PrimaryColor = null,
    [property: JsonPropertyName("secondaryColor")] string? SecondaryColor = null,
    [property: JsonPropertyName("logoUrl")] string? LogoUrl = null,
    [property: JsonPropertyName("faviconUrl")] string? FaviconUrl = null,
    [property: JsonPropertyName("customCss")] string? CustomCss = null
);

public record UpdateTenantConfigRequest(
    [property: JsonPropertyName("primaryColor")] string PrimaryColor,
    [property: JsonPropertyName("secondaryColor")] string SecondaryColor,
    [property: JsonPropertyName("logoUrl")] string LogoUrl
);

public record UpdateTenantRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("contactEmail")] string ContactEmail,
    [property: JsonPropertyName("contactPhone")] string ContactPhone,
    [property: JsonPropertyName("customDomain")] string? CustomDomain
);

public record UpdateTenantResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("tenant")] TenantDto Tenant
);

public record ActivateTenantResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("tenantId")] string TenantId,
    [property: JsonPropertyName("status")] string Status
);

public record TenantStatsResponse(
    [property: JsonPropertyName("totalTenants")] int TotalTenants,
    [property: JsonPropertyName("activeTenants")] int ActiveTenants,
    [property: JsonPropertyName("suspendedTenants")] int SuspendedTenants,
    [property: JsonPropertyName("inactiveTenants")] int InactiveTenants,
    [property: JsonPropertyName("tenantsByTier")] Dictionary<string, int> TenantsByTier
);

public record GetAllTenantsRequest(
    [property: JsonPropertyName("page")] int Page = 1,
    [property: JsonPropertyName("pageSize")] int PageSize = 10,
    [property: JsonPropertyName("status")] string? Status = null
);

public record PagedTenantsResponse(
    [property: JsonPropertyName("tenants")] IEnumerable<TenantDto> Tenants,
    [property: JsonPropertyName("totalCount")] int TotalCount,
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("pageSize")] int PageSize,
    [property: JsonPropertyName("totalPages")] int TotalPages
);

