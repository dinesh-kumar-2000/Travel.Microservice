using Dapper;
using TenantService.Domain.Entities;
using TenantService.Domain.Repositories;
using SharedKernel.Data;
using SharedKernel.Utilities;
using System.Text.Json;

namespace TenantService.Infrastructure.Repositories;

public class LandingPageRepository : ILandingPageRepository
{
    private readonly IDapperContext _context;

    public LandingPageRepository(IDapperContext context)
    {
        _context = context;
    }

    private static string GetSelectSql() => @"
        SELECT 
            page_id AS PageId,
            tenant_id AS TenantId,
            page_name AS PageName,
            slug AS Slug,
            status AS Status,
            language AS Language,
            seo_title AS SeoTitle,
            seo_description AS SeoDescription,
            seo_keywords AS SeoKeywords,
            seo_canonical_url AS SeoCanonicalUrl,
            seo_og_title AS SeoOgTitle,
            seo_og_description AS SeoOgDescription,
            seo_og_image AS SeoOgImage,
            seo_og_type AS SeoOgType,
            theme_primary_color AS ThemePrimaryColor,
            theme_secondary_color AS ThemeSecondaryColor,
            theme_font_family AS ThemeFontFamily,
            theme_background_color AS ThemeBackgroundColor,
            theme_text_color AS ThemeTextColor,
            theme_button_style AS ThemeButtonStyle,
            theme_button_hover_color AS ThemeButtonHoverColor,
            layout_container_width AS LayoutContainerWidth,
            layout_grid_system AS LayoutGridSystem,
            layout_section_padding AS LayoutSectionPadding,
            layout_component_gap AS LayoutComponentGap,
            layout_breakpoints AS LayoutBreakpoints,
            sections AS Sections,
            media_library AS MediaLibrary,
            custom_scripts AS CustomScripts,
            permissions AS Permissions,
            created_at AS CreatedAt,
            created_by AS CreatedBy,
            last_modified AS LastModified,
            modified_by AS ModifiedBy,
            published_at AS PublishedAt,
            version AS Version
        FROM landing_pages";

    public async Task<LandingPage?> GetByIdAsync(string pageId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var sql = GetSelectSql() + " WHERE page_id = @PageId";
        var data = await connection.QueryFirstOrDefaultAsync<LandingPageData>(sql, new { PageId = pageId });
        return data?.ToEntity();
    }

    public async Task<LandingPage?> GetBySlugAsync(string tenantId, string slug, string language = "en", CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var sql = GetSelectSql() + " WHERE tenant_id = @TenantId AND slug = @Slug AND language = @Language";
        var data = await connection.QueryFirstOrDefaultAsync<LandingPageData>(sql, new { TenantId = tenantId, Slug = slug, Language = language });
        return data?.ToEntity();
    }

    public async Task<IEnumerable<LandingPage>> GetByTenantIdAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var sql = GetSelectSql() + " WHERE tenant_id = @TenantId ORDER BY last_modified DESC";
        var data = await connection.QueryAsync<LandingPageData>(sql, new { TenantId = tenantId });
        return data.Select(d => d.ToEntity());
    }

    public async Task<IEnumerable<LandingPage>> GetPublishedByTenantIdAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var sql = GetSelectSql() + " WHERE tenant_id = @TenantId AND status = 'Published' ORDER BY published_at DESC";
        var data = await connection.QueryAsync<LandingPageData>(sql, new { TenantId = tenantId });
        return data.Select(d => d.ToEntity());
    }

    public async Task<IEnumerable<LandingPage>> SearchAsync(string tenantId, string? searchTerm, string? status, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var offset = (page - 1) * pageSize;
        
        var conditions = new List<string> { "tenant_id = @TenantId" };
        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", pageSize);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            conditions.Add("(page_name ILIKE @SearchTerm OR seo_title ILIKE @SearchTerm OR slug ILIKE @SearchTerm)");
            parameters.Add("SearchTerm", $"%{searchTerm}%");
        }

        if (!string.IsNullOrEmpty(status))
        {
            conditions.Add("status = @Status");
            parameters.Add("Status", status);
        }

        var sql = GetSelectSql() + $@"
            WHERE {string.Join(" AND ", conditions)}
            ORDER BY last_modified DESC
            LIMIT @PageSize OFFSET @Offset";

        var data = await connection.QueryAsync<LandingPageData>(sql, parameters);
        return data.Select(d => d.ToEntity());
    }

    public async Task<bool> ExistsAsync(string pageId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM landing_pages WHERE page_id = @PageId", new { PageId = pageId });
        return count > 0;
    }

    public async Task<bool> SlugExistsAsync(string tenantId, string slug, string language, string? excludePageId = null, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT COUNT(1) FROM landing_pages WHERE tenant_id = @TenantId AND slug = @Slug AND language = @Language";
        
        if (!string.IsNullOrEmpty(excludePageId))
        {
            sql += " AND page_id != @ExcludePageId";
        }

        var parameters = new { TenantId = tenantId, Slug = slug, Language = language, ExcludePageId = excludePageId };
        var count = await connection.ExecuteScalarAsync<int>(sql, parameters);
        return count > 0;
    }

    public async Task<LandingPage> AddAsync(LandingPage page, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var data = LandingPageData.FromEntity(page);
        
        var sql = @"
            INSERT INTO landing_pages (
                page_id, tenant_id, page_name, slug, status, language,
                seo_title, seo_description, seo_keywords, seo_canonical_url,
                seo_og_title, seo_og_description, seo_og_image, seo_og_type,
                theme_primary_color, theme_secondary_color, theme_font_family,
                theme_background_color, theme_text_color, theme_button_style, theme_button_hover_color,
                layout_container_width, layout_grid_system, layout_section_padding,
                layout_component_gap, layout_breakpoints,
                sections, media_library, custom_scripts, permissions,
                created_at, created_by, last_modified, modified_by, version
            ) VALUES (
                @PageId, @TenantId, @PageName, @Slug, @Status, @Language,
                @SeoTitle, @SeoDescription, @SeoKeywords::jsonb, @SeoCanonicalUrl,
                @SeoOgTitle, @SeoOgDescription, @SeoOgImage, @SeoOgType,
                @ThemePrimaryColor, @ThemeSecondaryColor, @ThemeFontFamily,
                @ThemeBackgroundColor, @ThemeTextColor, @ThemeButtonStyle, @ThemeButtonHoverColor,
                @LayoutContainerWidth, @LayoutGridSystem, @LayoutSectionPadding,
                @LayoutComponentGap, @LayoutBreakpoints::jsonb,
                @Sections::jsonb, @MediaLibrary::jsonb, @CustomScripts::jsonb, @Permissions::jsonb,
                @CreatedAt, @CreatedBy, @LastModified, @ModifiedBy, @Version
            )";
        
        await connection.ExecuteAsync(sql, data);
        return page;
    }

    public async Task<LandingPage> UpdateAsync(LandingPage page, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var data = LandingPageData.FromEntity(page);
        
        var sql = @"
            UPDATE landing_pages SET
                page_name = @PageName, slug = @Slug, status = @Status, language = @Language,
                seo_title = @SeoTitle, seo_description = @SeoDescription, seo_keywords = @SeoKeywords::jsonb,
                seo_canonical_url = @SeoCanonicalUrl, seo_og_title = @SeoOgTitle,
                seo_og_description = @SeoOgDescription, seo_og_image = @SeoOgImage, seo_og_type = @SeoOgType,
                theme_primary_color = @ThemePrimaryColor, theme_secondary_color = @ThemeSecondaryColor,
                theme_font_family = @ThemeFontFamily, theme_background_color = @ThemeBackgroundColor,
                theme_text_color = @ThemeTextColor, theme_button_style = @ThemeButtonStyle,
                theme_button_hover_color = @ThemeButtonHoverColor,
                layout_container_width = @LayoutContainerWidth, layout_grid_system = @LayoutGridSystem,
                layout_section_padding = @LayoutSectionPadding, layout_component_gap = @LayoutComponentGap,
                layout_breakpoints = @LayoutBreakpoints::jsonb,
                sections = @Sections::jsonb, media_library = @MediaLibrary::jsonb,
                custom_scripts = @CustomScripts::jsonb, permissions = @Permissions::jsonb,
                last_modified = @LastModified, modified_by = @ModifiedBy,
                published_at = @PublishedAt, version = @Version
            WHERE page_id = @PageId";
        
        await connection.ExecuteAsync(sql, data);
        return page;
    }

    public async Task<bool> DeleteAsync(string pageId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var result = await connection.ExecuteAsync("DELETE FROM landing_pages WHERE page_id = @PageId", new { PageId = pageId });
        return result > 0;
    }

    public async Task<int> GetTotalCountAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM landing_pages WHERE tenant_id = @TenantId", new { TenantId = tenantId });
    }

    private class LandingPageData
    {
        public string PageId { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string PageName { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Status { get; set; } = "Draft";
        public string Language { get; set; } = "en";
        
        public string? SeoTitle { get; set; }
        public string? SeoDescription { get; set; }
        public string? SeoKeywords { get; set; }
        public string? SeoCanonicalUrl { get; set; }
        public string? SeoOgTitle { get; set; }
        public string? SeoOgDescription { get; set; }
        public string? SeoOgImage { get; set; }
        public string? SeoOgType { get; set; }
        
        public string? ThemePrimaryColor { get; set; }
        public string? ThemeSecondaryColor { get; set; }
        public string? ThemeFontFamily { get; set; }
        public string? ThemeBackgroundColor { get; set; }
        public string? ThemeTextColor { get; set; }
        public string? ThemeButtonStyle { get; set; }
        public string? ThemeButtonHoverColor { get; set; }
        
        public string? LayoutContainerWidth { get; set; }
        public string? LayoutGridSystem { get; set; }
        public string? LayoutSectionPadding { get; set; }
        public string? LayoutComponentGap { get; set; }
        public string? LayoutBreakpoints { get; set; }
        
        public string Sections { get; set; } = "[]";
        public string MediaLibrary { get; set; } = "[]";
        public string CustomScripts { get; set; } = "[]";
        public string Permissions { get; set; } = "{}";
        
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime LastModified { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int Version { get; set; }

        public LandingPage ToEntity()
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var page = LandingPage.Create(PageId, TenantId, PageName, Slug, Language, CreatedBy);
            
            // Set status
            typeof(LandingPage).GetProperty("Status")!.SetValue(page, Enum.Parse<PageStatus>(Status));
            
            // Set SEO
            if (!string.IsNullOrEmpty(SeoTitle))
            {
                var seo = new SeoConfiguration
                {
                    Title = SeoTitle,
                    Description = SeoDescription,
                    Keywords = string.IsNullOrEmpty(SeoKeywords) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(SeoKeywords, jsonOptions) ?? new List<string>(),
                    CanonicalUrl = SeoCanonicalUrl,
                    OpenGraph = new OpenGraphConfig
                    {
                        OgTitle = SeoOgTitle,
                        OgDescription = SeoOgDescription,
                        OgImage = SeoOgImage,
                        OgType = SeoOgType
                    }
                };
                page.UpdateSeo(seo);
            }
            
            // Set Theme
            if (!string.IsNullOrEmpty(ThemePrimaryColor))
            {
                var theme = new ThemeConfiguration
                {
                    PrimaryColor = ThemePrimaryColor,
                    SecondaryColor = ThemeSecondaryColor,
                    FontFamily = ThemeFontFamily,
                    BackgroundColor = ThemeBackgroundColor,
                    TextColor = ThemeTextColor,
                    Button = new ButtonStyle
                    {
                        Style = ThemeButtonStyle,
                        HoverColor = ThemeButtonHoverColor
                    }
                };
                page.UpdateTheme(theme);
            }
            
            // Set Layout
            if (!string.IsNullOrEmpty(LayoutContainerWidth))
            {
                var layout = new LayoutConfiguration
                {
                    ContainerWidth = LayoutContainerWidth,
                    GridSystem = LayoutGridSystem,
                    Spacing = new SpacingConfig
                    {
                        SectionPadding = LayoutSectionPadding,
                        ComponentGap = LayoutComponentGap
                    },
                    Breakpoints = string.IsNullOrEmpty(LayoutBreakpoints) 
                        ? new BreakpointConfig() 
                        : JsonSerializer.Deserialize<BreakpointConfig>(LayoutBreakpoints, jsonOptions) ?? new BreakpointConfig()
                };
                page.UpdateLayout(layout);
            }
            
            // Set Sections - deserialize as dynamic first to preserve all properties
            var sectionsJson = JsonSerializer.Deserialize<List<JsonElement>>(Sections, jsonOptions) ?? new List<JsonElement>();
            var sections = new List<PageSection>();
            
            foreach (var sectionElement in sectionsJson)
            {
                var section = new PageSection
                {
                    SectionId = sectionElement.TryGetProperty("sectionId", out var sid) ? sid.GetString() ?? "" : "",
                    Type = sectionElement.TryGetProperty("type", out var type) ? type.GetString() ?? "" : "",
                    Order = sectionElement.TryGetProperty("order", out var order) ? order.GetInt32() : 0,
                    Visible = sectionElement.TryGetProperty("visible", out var visible) ? visible.GetBoolean() : true,
                    Content = sectionElement.TryGetProperty("content", out var content) 
                        ? JsonSerializer.Deserialize<Dictionary<string, object>>(content.GetRawText(), jsonOptions) ?? new Dictionary<string, object>()
                        : new Dictionary<string, object>(),
                    Background = sectionElement.TryGetProperty("background", out var bg)
                        ? JsonSerializer.Deserialize<Dictionary<string, object>>(bg.GetRawText(), jsonOptions)
                        : null,
                    Animation = sectionElement.TryGetProperty("animation", out var anim)
                        ? JsonSerializer.Deserialize<Dictionary<string, object>>(anim.GetRawText(), jsonOptions)
                        : null,
                    AdditionalProps = new Dictionary<string, object>()
                };
                
                // Extract additional properties
                if (sectionElement.TryGetProperty("title", out var title))
                    section.AdditionalProps["title"] = title.GetString() ?? "";
                if (sectionElement.TryGetProperty("columns", out var cols))
                    section.AdditionalProps["columns"] = cols.GetInt32();
                if (sectionElement.TryGetProperty("cards", out var cards))
                    section.AdditionalProps["cards"] = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(cards.GetRawText(), jsonOptions) ?? new List<Dictionary<string, object>>();
                if (sectionElement.TryGetProperty("items", out var items))
                    section.AdditionalProps["items"] = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(items.GetRawText(), jsonOptions) ?? new List<Dictionary<string, object>>();
                
                sections.Add(section);
            }
            
            page.SetSections(sections);
            
            // Set Media Library
            var mediaLibrary = JsonSerializer.Deserialize<List<MediaAsset>>(MediaLibrary, jsonOptions) ?? new List<MediaAsset>();
            page.UpdateMediaLibrary(mediaLibrary);
            
            // Set Custom Scripts
            var customScripts = JsonSerializer.Deserialize<List<CustomScript>>(CustomScripts, jsonOptions) ?? new List<CustomScript>();
            page.UpdateCustomScripts(customScripts);
            
            // Set Permissions
            var permissions = JsonSerializer.Deserialize<PagePermissions>(Permissions, jsonOptions) ?? PagePermissions.Default();
            typeof(LandingPage).GetProperty("Permissions")!.SetValue(page, permissions);
            
            // Set audit fields
            typeof(LandingPage).GetProperty("LastModified")!.SetValue(page, LastModified);
            typeof(LandingPage).GetProperty("ModifiedBy")!.SetValue(page, ModifiedBy);
            typeof(LandingPage).GetProperty("PublishedAt")!.SetValue(page, PublishedAt);
            typeof(LandingPage).GetProperty("Version")!.SetValue(page, Version);
            typeof(LandingPage).GetProperty("CreatedAt")!.SetValue(page, CreatedAt);
            
            return page;
        }

        public static LandingPageData FromEntity(LandingPage entity)
        {
            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            
            return new LandingPageData
            {
                PageId = entity.Id,
                TenantId = entity.TenantId,
                PageName = entity.PageName,
                Slug = entity.Slug,
                Status = entity.Status.ToString(),
                Language = entity.Language,
                
                SeoTitle = entity.Seo?.Title,
                SeoDescription = entity.Seo?.Description,
                SeoKeywords = entity.Seo?.Keywords != null ? JsonSerializer.Serialize(entity.Seo.Keywords, jsonOptions) : null,
                SeoCanonicalUrl = entity.Seo?.CanonicalUrl,
                SeoOgTitle = entity.Seo?.OpenGraph?.OgTitle,
                SeoOgDescription = entity.Seo?.OpenGraph?.OgDescription,
                SeoOgImage = entity.Seo?.OpenGraph?.OgImage,
                SeoOgType = entity.Seo?.OpenGraph?.OgType,
                
                ThemePrimaryColor = entity.Theme?.PrimaryColor,
                ThemeSecondaryColor = entity.Theme?.SecondaryColor,
                ThemeFontFamily = entity.Theme?.FontFamily,
                ThemeBackgroundColor = entity.Theme?.BackgroundColor,
                ThemeTextColor = entity.Theme?.TextColor,
                ThemeButtonStyle = entity.Theme?.Button?.Style,
                ThemeButtonHoverColor = entity.Theme?.Button?.HoverColor,
                
                LayoutContainerWidth = entity.Layout?.ContainerWidth,
                LayoutGridSystem = entity.Layout?.GridSystem,
                LayoutSectionPadding = entity.Layout?.Spacing?.SectionPadding,
                LayoutComponentGap = entity.Layout?.Spacing?.ComponentGap,
                LayoutBreakpoints = entity.Layout?.Breakpoints != null ? JsonSerializer.Serialize(entity.Layout.Breakpoints, jsonOptions) : null,
                
                Sections = JsonSerializer.Serialize(entity.Sections, jsonOptions),
                MediaLibrary = JsonSerializer.Serialize(entity.MediaLibrary, jsonOptions),
                CustomScripts = JsonSerializer.Serialize(entity.CustomScripts, jsonOptions),
                Permissions = JsonSerializer.Serialize(entity.Permissions, jsonOptions),
                
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                LastModified = entity.LastModified,
                ModifiedBy = entity.ModifiedBy,
                PublishedAt = entity.PublishedAt,
                Version = entity.Version
            };
        }
    }
}

