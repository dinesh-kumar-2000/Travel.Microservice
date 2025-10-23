using SharedKernel.Models;

namespace SeoInsuranceService.Domain.Entities;

public class SEOSettings : TenantEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;

    // General SEO
    public string? SiteTitle { get; set; }
    public string? SiteDescription { get; set; }
    public string? Keywords { get; set; }
    public string? Author { get; set; }
    public string? CanonicalUrl { get; set; }

    // Open Graph
    public string? OgTitle { get; set; }
    public string? OgDescription { get; set; }
    public string? OgImage { get; set; }
    public string? OgType { get; set; }
    public string? OgUrl { get; set; }

    // Twitter
    public string? TwitterCard { get; set; }
    public string? TwitterSite { get; set; }
    public string? TwitterTitle { get; set; }
    public string? TwitterDescription { get; set; }
    public string? TwitterImage { get; set; }

    // Technical
    public string? Robots { get; set; }
    public string? GoogleSiteVerification { get; set; }
    public string? BingSiteVerification { get; set; }
    public string? GoogleAnalyticsId { get; set; }
    public string? GtmId { get; set; }

    // Sitemap
    public bool EnableSitemap { get; set; } = true;
    public string? SitemapUrl { get; set; }
    public DateTime? LastSitemapGenerated { get; set; }

    // Schema
    public bool EnableSchema { get; set; } = true;
    public string? OrganizationSchema { get; set; }
    public bool BreadcrumbSchema { get; set; } = true;
    public bool ProductSchema { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}

