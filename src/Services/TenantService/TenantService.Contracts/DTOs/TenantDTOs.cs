namespace TenantService.Contracts.DTOs;

public record CreateTenantRequest(
    string Name,
    string Subdomain,
    string ContactEmail,
    string ContactPhone
);

public record TenantDto(
    string Id,
    string Name,
    string Subdomain,
    string ContactEmail,
    string Status,
    string Tier
);

public record UpdateTenantConfigRequest(
    string PrimaryColor,
    string SecondaryColor,
    string LogoUrl
);

