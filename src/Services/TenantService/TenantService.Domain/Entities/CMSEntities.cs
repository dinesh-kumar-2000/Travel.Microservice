namespace TenantService.Domain.Entities;

public class BlogPost
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Excerpt { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public List<string>? Tags { get; set; }
    public string? FeaturedImage { get; set; }
    public string Status { get; set; } = "draft";
    public int Views { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public void Publish()
    {
        Status = "published";
        PublishedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        Status = "archived";
    }

    public void IncrementViews()
    {
        Views++;
    }
}

public class FAQ
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CMSPage
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string PageType { get; set; } = "custom";
    public string Status { get; set; } = "draft";
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public bool IsSystemPage()
    {
        return PageType is "terms" or "privacy" or "about" or "contact";
    }
}

public class EmailTemplate
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TemplateType { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string> Variables { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string RenderContent(Dictionary<string, string> values)
    {
        var rendered = Content;
        foreach (var kvp in values)
        {
            rendered = rendered.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
        }
        return rendered;
    }

    public string RenderSubject(Dictionary<string, string> values)
    {
        var rendered = Subject;
        foreach (var kvp in values)
        {
            rendered = rendered.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
        }
        return rendered;
    }
}

public class SMSTemplate
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TemplateType { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string Content { get; set; } = string.Empty;
    public List<string> Variables { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string RenderContent(Dictionary<string, string> values)
    {
        var rendered = Content;
        foreach (var kvp in values)
        {
            rendered = rendered.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
        }
        return rendered;
    }

    public bool IsWithinSMSLimit()
    {
        return Content.Length <= 160; // Standard SMS length
    }
}

public class SEOSettings
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }

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
}

