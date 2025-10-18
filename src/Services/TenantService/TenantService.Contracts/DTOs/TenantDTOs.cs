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
    [property: JsonPropertyName("displayName")] string DisplayName,
    [property: JsonPropertyName("subdomain")] string Subdomain,
    [property: JsonPropertyName("contactEmail")] string ContactEmail,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("tier")] string Tier,
    [property: JsonPropertyName("logoUrl")] string? LogoUrl,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("isActive")] bool IsActive
);

public record UpdateTenantConfigRequest(
    [property: JsonPropertyName("primaryColor")] string PrimaryColor,
    [property: JsonPropertyName("secondaryColor")] string SecondaryColor,
    [property: JsonPropertyName("logoUrl")] string LogoUrl
);

