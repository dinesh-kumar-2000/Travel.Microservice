using SharedKernel.Models;

namespace CmsService.Domain.Entities;

public class LandingPage : BaseEntity<string>
{
    public string TenantId { get; private set; } = string.Empty;
    public string PageName { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public PageStatus Status { get; private set; } = PageStatus.Draft;
    public string Language { get; private set; } = "en";
    
    public SeoConfiguration? Seo { get; private set; }
    public ThemeConfiguration? Theme { get; private set; }
    public LayoutConfiguration? Layout { get; private set; }
    
    public List<PageSection> Sections { get; private set; } = new();
    public List<MediaAsset> MediaLibrary { get; private set; } = new();
    public List<CustomScript> CustomScripts { get; private set; } = new();
    
    public PagePermissions Permissions { get; private set; } = new();
    
    public DateTime LastModified { get; private set; }
    public string? ModifiedBy { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public int Version { get; private set; } = 1;

    private LandingPage() { }

    public static LandingPage Create(
        string pageId,
        string tenantId,
        string pageName,
        string slug,
        string language = "en",
        string? createdBy = null)
    {
        return new LandingPage
        {
            Id = pageId,
            TenantId = tenantId,
            PageName = pageName,
            Slug = slug.StartsWith('/') ? slug : $"/{slug}",
            Language = language,
            Status = PageStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            LastModified = DateTime.UtcNow,
            CreatedBy = createdBy,
            ModifiedBy = createdBy,
            Permissions = PagePermissions.Default()
        };
    }

    public void UpdateBasicInfo(string pageName, string slug, string language, string? modifiedBy = null)
    {
        PageName = pageName;
        Slug = slug.StartsWith('/') ? slug : $"/{slug}";
        Language = language;
        LastModified = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSeo(SeoConfiguration seo, string? modifiedBy = null)
    {
        Seo = seo;
        LastModified = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateTheme(ThemeConfiguration theme, string? modifiedBy = null)
    {
        Theme = theme;
        LastModified = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateLayout(LayoutConfiguration layout, string? modifiedBy = null)
    {
        Layout = layout;
        LastModified = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSections(List<PageSection> sections, string? modifiedBy = null)
    {
        Sections = sections.OrderBy(s => s.Order).ToList();
        LastModified = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddSection(PageSection section, string? modifiedBy = null)
    {
        Sections.Add(section);
        Sections = Sections.OrderBy(s => s.Order).ToList();
        LastModified = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveSection(string sectionId, string? modifiedBy = null)
    {
        Sections.RemoveAll(s => s.SectionId == sectionId);
        LastModified = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateMediaLibrary(List<MediaAsset> mediaLibrary, string? modifiedBy = null)
    {
        MediaLibrary = mediaLibrary;
        LastModified = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCustomScripts(List<CustomScript> customScripts, string? modifiedBy = null)
    {
        CustomScripts = customScripts;
        LastModified = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Publish(string? modifiedBy = null)
    {
        Status = PageStatus.Published;
        PublishedAt = DateTime.UtcNow;
        LastModified = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unpublish(string? modifiedBy = null)
    {
        Status = PageStatus.Draft;
        LastModified = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Archive(string? modifiedBy = null)
    {
        Status = PageStatus.Archived;
        LastModified = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementVersion()
    {
        Version++;
    }
}

public enum PageStatus
{
    Draft,
    Published,
    Archived
}

public class SeoConfiguration
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public List<string> Keywords { get; set; } = new();
    public string? CanonicalUrl { get; set; }
    public OpenGraphConfig? OpenGraph { get; set; }
}

public class OpenGraphConfig
{
    public string? OgTitle { get; set; }
    public string? OgDescription { get; set; }
    public string? OgImage { get; set; }
    public string? OgType { get; set; }
}

public class ThemeConfiguration
{
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? FontFamily { get; set; }
    public string? BackgroundColor { get; set; }
    public string? TextColor { get; set; }
    public ButtonStyle? Button { get; set; }
}

public class ButtonStyle
{
    public string? Style { get; set; }
    public string? HoverColor { get; set; }
}

public class LayoutConfiguration
{
    public string? ContainerWidth { get; set; }
    public string? GridSystem { get; set; }
    public SpacingConfig? Spacing { get; set; }
    public BreakpointConfig? Breakpoints { get; set; }
}

public class SpacingConfig
{
    public string? SectionPadding { get; set; }
    public string? ComponentGap { get; set; }
}

public class BreakpointConfig
{
    public int Mobile { get; set; } = 480;
    public int Tablet { get; set; } = 768;
    public int Desktop { get; set; } = 1200;
}

public class PageSection
{
    public string SectionId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool Visible { get; set; } = true;
    public Dictionary<string, object> Content { get; set; } = new();
    public Dictionary<string, object>? Background { get; set; }
    public Dictionary<string, object>? Animation { get; set; }
    public Dictionary<string, object>? AdditionalProps { get; set; }
}

public class MediaAsset
{
    public string Id { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // image, video, etc.
    public string? AltText { get; set; }
    public string? Caption { get; set; }
}

public class CustomScript
{
    public string Trigger { get; set; } = string.Empty; // onLoad, onScroll, etc.
    public string Script { get; set; } = string.Empty;
}

public class PagePermissions
{
    public List<string> EditableBy { get; set; } = new();
    public List<string> ViewableBy { get; set; } = new();

    public static PagePermissions Default()
    {
        return new PagePermissions
        {
            EditableBy = new List<string> { "TenantAdmin", "SuperAdmin" },
            ViewableBy = new List<string> { "Public", "TenantAdmin", "SuperAdmin" }
        };
    }
}

