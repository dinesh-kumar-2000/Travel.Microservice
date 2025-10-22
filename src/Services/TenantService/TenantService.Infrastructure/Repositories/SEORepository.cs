using Dapper;
using TenantService.Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Data;
using SharedKernel.Utilities;
using Tenancy;

namespace TenantService.Infrastructure.Repositories;

public interface ISEORepository
{
    Task<SEOSettings?> GetByTenantIdAsync(Guid tenantId);
    Task<SEOSettings> CreateOrUpdateAsync(SEOSettings settings);
    Task<bool> GenerateSitemapAsync(Guid tenantId);
}

public class SEORepository : ISEORepository
{
    private readonly IDapperContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<SEORepository> _logger;

    public SEORepository(
        IDapperContext context,
        ITenantContext tenantContext,
        ILogger<SEORepository> logger)
    {
        _context = context;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<SEOSettings?> GetByTenantIdAsync(Guid tenantId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   site_title AS SiteTitle,
                   site_description AS SiteDescription,
                   keywords AS Keywords,
                   author AS Author,
                   canonical_url AS CanonicalUrl,
                   og_title AS OgTitle,
                   og_description AS OgDescription,
                   og_image AS OgImage,
                   og_type AS OgType,
                   og_url AS OgUrl,
                   twitter_card AS TwitterCard,
                   twitter_site AS TwitterSite,
                   twitter_title AS TwitterTitle,
                   twitter_description AS TwitterDescription,
                   twitter_image AS TwitterImage,
                   robots AS Robots,
                   google_site_verification AS GoogleSiteVerification,
                   bing_site_verification AS BingSiteVerification,
                   google_analytics_id AS GoogleAnalyticsId,
                   gtm_id AS GtmId,
                   enable_sitemap AS EnableSitemap,
                   sitemap_url AS SitemapUrl,
                   last_sitemap_generated AS LastSitemapGenerated,
                   enable_schema AS EnableSchema,
                   organization_schema AS OrganizationSchema,
                   breadcrumb_schema AS BreadcrumbSchema,
                   product_schema AS ProductSchema,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM seo_settings
            WHERE tenant_id = @TenantId";

        return await connection.QueryFirstOrDefaultAsync<SEOSettings>(sql, new { TenantId = tenantId });
    }

    public async Task<SEOSettings> CreateOrUpdateAsync(SEOSettings settings)
    {
        using var connection = _context.CreateConnection();
        
        var existing = await GetByTenantIdAsync(settings.TenantId);

        if (existing == null)
        {
            settings.Id = Guid.NewGuid();
            settings.CreatedAt = DefaultProviders.DateTimeProvider.UtcNow;
            settings.UpdatedAt = DefaultProviders.DateTimeProvider.UtcNow;

            const string insertSql = @"
                INSERT INTO seo_settings (
                    id, tenant_id, site_title, site_description, keywords, author, canonical_url,
                    og_title, og_description, og_image, og_type, og_url,
                    twitter_card, twitter_site, twitter_title, twitter_description, twitter_image,
                    robots, google_site_verification, bing_site_verification, google_analytics_id, gtm_id,
                    enable_sitemap, sitemap_url, last_sitemap_generated,
                    enable_schema, organization_schema, breadcrumb_schema, product_schema,
                    created_at, updated_at
                )
                VALUES (
                    @Id, @TenantId, @SiteTitle, @SiteDescription, @Keywords, @Author, @CanonicalUrl,
                    @OgTitle, @OgDescription, @OgImage, @OgType, @OgUrl,
                    @TwitterCard, @TwitterSite, @TwitterTitle, @TwitterDescription, @TwitterImage,
                    @Robots, @GoogleSiteVerification, @BingSiteVerification, @GoogleAnalyticsId, @GtmId,
                    @EnableSitemap, @SitemapUrl, @LastSitemapGenerated,
                    @EnableSchema, @OrganizationSchema, @BreadcrumbSchema, @ProductSchema,
                    @CreatedAt, @UpdatedAt
                )";

            await connection.ExecuteAsync(insertSql, settings);
            _logger.LogInformation("SEO settings created for tenant {TenantId}", settings.TenantId);
        }
        else
        {
            settings.Id = existing.Id;
            settings.CreatedAt = existing.CreatedAt;
            settings.UpdatedAt = DefaultProviders.DateTimeProvider.UtcNow;

            const string updateSql = @"
                UPDATE seo_settings SET
                    site_title = @SiteTitle,
                    site_description = @SiteDescription,
                    keywords = @Keywords,
                    author = @Author,
                    canonical_url = @CanonicalUrl,
                    og_title = @OgTitle,
                    og_description = @OgDescription,
                    og_image = @OgImage,
                    og_type = @OgType,
                    og_url = @OgUrl,
                    twitter_card = @TwitterCard,
                    twitter_site = @TwitterSite,
                    twitter_title = @TwitterTitle,
                    twitter_description = @TwitterDescription,
                    twitter_image = @TwitterImage,
                    robots = @Robots,
                    google_site_verification = @GoogleSiteVerification,
                    bing_site_verification = @BingSiteVerification,
                    google_analytics_id = @GoogleAnalyticsId,
                    gtm_id = @GtmId,
                    enable_sitemap = @EnableSitemap,
                    enable_schema = @EnableSchema,
                    organization_schema = @OrganizationSchema,
                    breadcrumb_schema = @BreadcrumbSchema,
                    product_schema = @ProductSchema,
                    updated_at = @UpdatedAt
                WHERE id = @Id AND tenant_id = @TenantId";

            await connection.ExecuteAsync(updateSql, settings);
            _logger.LogInformation("SEO settings updated for tenant {TenantId}", settings.TenantId);
        }

        return settings;
    }

    public async Task<bool> GenerateSitemapAsync(Guid tenantId)
    {
        using var connection = _context.CreateConnection();
        
        var sitemapUrl = $"https://cdn.example.com/sitemaps/{tenantId}/sitemap.xml";

        const string sql = @"
            UPDATE seo_settings 
            SET sitemap_url = @SitemapUrl,
                last_sitemap_generated = @GeneratedAt,
                updated_at = @UpdatedAt
            WHERE tenant_id = @TenantId";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            TenantId = tenantId,
            SitemapUrl = sitemapUrl,
            GeneratedAt = DefaultProviders.DateTimeProvider.UtcNow,
            UpdatedAt = DefaultProviders.DateTimeProvider.UtcNow
        });

        if (rowsAffected > 0)
        {
            _logger.LogInformation("Sitemap generated for tenant {TenantId}", tenantId);
        }

        return rowsAffected > 0;
    }
}

// SMS Template Repository
public interface ISMSTemplateRepository
{
    Task<SMSTemplate?> GetSMSTemplateByIdAsync(Guid id, Guid tenantId);
    Task<List<SMSTemplate>> GetSMSTemplatesAsync(Guid tenantId);
    Task<SMSTemplate> CreateSMSTemplateAsync(SMSTemplate template);
    Task<SMSTemplate?> UpdateSMSTemplateAsync(SMSTemplate template);
    Task<bool> DeleteSMSTemplateAsync(Guid id, Guid tenantId);
}

public class SMSTemplateRepository : ISMSTemplateRepository
{
    private readonly IDapperContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<SMSTemplateRepository> _logger;

    public SMSTemplateRepository(
        IDapperContext context,
        ITenantContext tenantContext,
        ILogger<SMSTemplateRepository> logger)
    {
        _context = context;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<SMSTemplate?> GetSMSTemplateByIdAsync(Guid id, Guid tenantId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   name AS Name,
                   template_type AS TemplateType,
                   category AS Category,
                   content AS Content,
                   variables AS Variables,
                   is_active AS IsActive,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM sms_templates
            WHERE id = @Id AND tenant_id = @TenantId";

        return await connection.QueryFirstOrDefaultAsync<SMSTemplate>(sql, new { Id = id, TenantId = tenantId });
    }

    public async Task<List<SMSTemplate>> GetSMSTemplatesAsync(Guid tenantId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   name AS Name,
                   template_type AS TemplateType,
                   category AS Category,
                   content AS Content,
                   variables AS Variables,
                   is_active AS IsActive,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM sms_templates
            WHERE tenant_id = @TenantId
            ORDER BY name ASC";

        var templates = await connection.QueryAsync<SMSTemplate>(sql, new { TenantId = tenantId });
        return templates.ToList();
    }

    public async Task<SMSTemplate> CreateSMSTemplateAsync(SMSTemplate template)
    {
        using var connection = _context.CreateConnection();
        
        template.Id = Guid.NewGuid();
        template.CreatedAt = DefaultProviders.DateTimeProvider.UtcNow;
        template.UpdatedAt = DefaultProviders.DateTimeProvider.UtcNow;

        const string sql = @"
            INSERT INTO sms_templates (
                id, tenant_id, name, template_type, category, content,
                variables, is_active, created_at, updated_at
            )
            VALUES (
                @Id, @TenantId, @Name, @TemplateType, @Category, @Content,
                @Variables, @IsActive, @CreatedAt, @UpdatedAt
            )";

        await connection.ExecuteAsync(sql, template);

        _logger.LogInformation("SMS template created: {Name}", template.Name);
        return template;
    }

    public async Task<SMSTemplate?> UpdateSMSTemplateAsync(SMSTemplate template)
    {
        using var connection = _context.CreateConnection();
        
        template.UpdatedAt = DefaultProviders.DateTimeProvider.UtcNow;

        const string sql = @"
            UPDATE sms_templates SET
                name = @Name,
                template_type = @TemplateType,
                category = @Category,
                content = @Content,
                variables = @Variables,
                is_active = @IsActive,
                updated_at = @UpdatedAt
            WHERE id = @Id AND tenant_id = @TenantId";

        var rowsAffected = await connection.ExecuteAsync(sql, template);
        return rowsAffected > 0 ? template : null;
    }

    public async Task<bool> DeleteSMSTemplateAsync(Guid id, Guid tenantId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = "DELETE FROM sms_templates WHERE id = @Id AND tenant_id = @TenantId";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, TenantId = tenantId });

        return rowsAffected > 0;
    }
}

