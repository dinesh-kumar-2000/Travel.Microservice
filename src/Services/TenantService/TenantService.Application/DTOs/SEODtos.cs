namespace TenantService.Application.DTOs;

public record SEOSettingsDto(
    Guid TenantId,
    // General
    string? SiteTitle,
    string? SiteDescription,
    string? Keywords,
    string? Author,
    string? CanonicalUrl,
    // Open Graph
    string? OgTitle,
    string? OgDescription,
    string? OgImage,
    string? OgType,
    string? OgUrl,
    // Twitter
    string? TwitterCard,
    string? TwitterSite,
    string? TwitterTitle,
    string? TwitterDescription,
    string? TwitterImage,
    // Technical
    string? Robots,
    string? GoogleSiteVerification,
    string? BingSiteVerification,
    string? GoogleAnalyticsId,
    string? GtmId,
    // Sitemap
    bool EnableSitemap,
    string? SitemapUrl,
    DateTime? LastSitemapGenerated,
    // Schema
    bool EnableSchema,
    string? OrganizationSchema,
    bool BreadcrumbSchema,
    bool ProductSchema,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record UpdateSEOSettingsRequest(
    GeneralSEODto General,
    OpenGraphDto OpenGraph,
    TwitterCardDto Twitter,
    TechnicalSEODto Technical,
    SitemapDto Sitemap,
    SchemaDto Schema
);

public record GeneralSEODto(
    string? SiteTitle,
    string? SiteDescription,
    string? Keywords,
    string? Author,
    string? CanonicalUrl
);

public record OpenGraphDto(
    string? OgTitle,
    string? OgDescription,
    string? OgImage,
    string? OgType,
    string? OgUrl
);

public record TwitterCardDto(
    string? TwitterCard,
    string? TwitterSite,
    string? TwitterTitle,
    string? TwitterDescription,
    string? TwitterImage
);

public record TechnicalSEODto(
    string? Robots,
    string? GoogleSiteVerification,
    string? BingSiteVerification,
    string? GoogleAnalyticsId,
    string? GtmId
);

public record SitemapDto(
    bool EnableSitemap
);

public record SchemaDto(
    bool EnableSchema,
    string? OrganizationSchema,
    bool BreadcrumbSchema,
    bool ProductSchema
);

public record SitemapResponseDto(
    bool Success,
    string SitemapUrl,
    int UrlCount,
    DateTime GeneratedAt
);

