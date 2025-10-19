using System.Text.Json.Serialization;

namespace TenantService.Contracts.DTOs;

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

