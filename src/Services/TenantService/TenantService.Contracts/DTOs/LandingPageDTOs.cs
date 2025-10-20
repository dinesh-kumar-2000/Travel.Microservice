using System.Text.Json.Serialization;

namespace TenantService.Contracts.DTOs;

// ================================================================================================
// REQUEST DTOs
// ================================================================================================

public record CreateLandingPageRequest(
    [property: JsonPropertyName("pageId")] string PageId,
    [property: JsonPropertyName("tenantId")] string TenantId,
    [property: JsonPropertyName("pageName")] string PageName,
    [property: JsonPropertyName("slug")] string Slug,
    [property: JsonPropertyName("language")] string Language = "en",
    [property: JsonPropertyName("seo")] SeoConfigDto? Seo = null,
    [property: JsonPropertyName("theme")] ThemeConfigDto? Theme = null,
    [property: JsonPropertyName("layout")] LayoutConfigDto? Layout = null,
    [property: JsonPropertyName("sections")] List<PageSectionDto>? Sections = null,
    [property: JsonPropertyName("mediaLibrary")] List<MediaAssetDto>? MediaLibrary = null,
    [property: JsonPropertyName("customScripts")] List<CustomScriptDto>? CustomScripts = null,
    [property: JsonPropertyName("permissions")] PagePermissionsDto? Permissions = null
);

public record UpdateLandingPageRequest(
    [property: JsonPropertyName("pageName")] string PageName,
    [property: JsonPropertyName("slug")] string Slug,
    [property: JsonPropertyName("language")] string Language,
    [property: JsonPropertyName("seo")] SeoConfigDto? Seo = null,
    [property: JsonPropertyName("theme")] ThemeConfigDto? Theme = null,
    [property: JsonPropertyName("layout")] LayoutConfigDto? Layout = null,
    [property: JsonPropertyName("sections")] List<PageSectionDto>? Sections = null,
    [property: JsonPropertyName("mediaLibrary")] List<MediaAssetDto>? MediaLibrary = null,
    [property: JsonPropertyName("customScripts")] List<CustomScriptDto>? CustomScripts = null,
    [property: JsonPropertyName("permissions")] PagePermissionsDto? Permissions = null
);

public record PublishLandingPageRequest(
    [property: JsonPropertyName("pageId")] string PageId
);

// ================================================================================================
// RESPONSE DTOs
// ================================================================================================

public record LandingPageDto(
    [property: JsonPropertyName("pageId")] string PageId,
    [property: JsonPropertyName("tenantId")] string TenantId,
    [property: JsonPropertyName("pageName")] string PageName,
    [property: JsonPropertyName("slug")] string Slug,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("language")] string Language,
    [property: JsonPropertyName("seo")] SeoConfigDto? Seo,
    [property: JsonPropertyName("theme")] ThemeConfigDto? Theme,
    [property: JsonPropertyName("layout")] LayoutConfigDto? Layout,
    [property: JsonPropertyName("sections")] List<PageSectionDto> Sections,
    [property: JsonPropertyName("mediaLibrary")] List<MediaAssetDto> MediaLibrary,
    [property: JsonPropertyName("customScripts")] List<CustomScriptDto> CustomScripts,
    [property: JsonPropertyName("permissions")] PagePermissionsDto Permissions,
    [property: JsonPropertyName("lastModified")] DateTime LastModified,
    [property: JsonPropertyName("publishedAt")] DateTime? PublishedAt,
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt,
    [property: JsonPropertyName("createdBy")] string? CreatedBy
);

public record LandingPageSummaryDto(
    [property: JsonPropertyName("pageId")] string PageId,
    [property: JsonPropertyName("tenantId")] string TenantId,
    [property: JsonPropertyName("pageName")] string PageName,
    [property: JsonPropertyName("slug")] string Slug,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("language")] string Language,
    [property: JsonPropertyName("lastModified")] DateTime LastModified,
    [property: JsonPropertyName("publishedAt")] DateTime? PublishedAt,
    [property: JsonPropertyName("version")] int Version
);

// ================================================================================================
// CONFIGURATION DTOs
// ================================================================================================

public record SeoConfigDto(
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("keywords")] List<string>? Keywords,
    [property: JsonPropertyName("canonicalUrl")] string? CanonicalUrl,
    [property: JsonPropertyName("openGraph")] OpenGraphDto? OpenGraph
);

public record OpenGraphDto(
    [property: JsonPropertyName("ogTitle")] string? OgTitle,
    [property: JsonPropertyName("ogDescription")] string? OgDescription,
    [property: JsonPropertyName("ogImage")] string? OgImage,
    [property: JsonPropertyName("ogType")] string? OgType
);

public record ThemeConfigDto(
    [property: JsonPropertyName("primaryColor")] string? PrimaryColor,
    [property: JsonPropertyName("secondaryColor")] string? SecondaryColor,
    [property: JsonPropertyName("fontFamily")] string? FontFamily,
    [property: JsonPropertyName("backgroundColor")] string? BackgroundColor,
    [property: JsonPropertyName("textColor")] string? TextColor,
    [property: JsonPropertyName("button")] ButtonStyleDto? Button
);

public record ButtonStyleDto(
    [property: JsonPropertyName("style")] string? Style,
    [property: JsonPropertyName("hoverColor")] string? HoverColor
);

public record LayoutConfigDto(
    [property: JsonPropertyName("containerWidth")] string? ContainerWidth,
    [property: JsonPropertyName("gridSystem")] string? GridSystem,
    [property: JsonPropertyName("spacing")] SpacingDto? Spacing,
    [property: JsonPropertyName("breakpoints")] BreakpointDto? Breakpoints
);

public record SpacingDto(
    [property: JsonPropertyName("sectionPadding")] string? SectionPadding,
    [property: JsonPropertyName("componentGap")] string? ComponentGap
);

public record BreakpointDto(
    [property: JsonPropertyName("mobile")] int Mobile,
    [property: JsonPropertyName("tablet")] int Tablet,
    [property: JsonPropertyName("desktop")] int Desktop
);

// ================================================================================================
// SECTION DTOs
// ================================================================================================

public record PageSectionDto(
    [property: JsonPropertyName("sectionId")] string SectionId,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("order")] int Order,
    [property: JsonPropertyName("visible")] bool Visible,
    [property: JsonPropertyName("content")] Dictionary<string, object>? Content,
    [property: JsonPropertyName("background")] Dictionary<string, object>? Background,
    [property: JsonPropertyName("animation")] Dictionary<string, object>? Animation,
    [property: JsonPropertyName("title")] string? Title = null,
    [property: JsonPropertyName("columns")] int? Columns = null,
    [property: JsonPropertyName("cards")] List<Dictionary<string, object>>? Cards = null,
    [property: JsonPropertyName("items")] List<Dictionary<string, object>>? Items = null
);

// ================================================================================================
// MEDIA & SCRIPTS DTOs
// ================================================================================================

public record MediaAssetDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("altText")] string? AltText,
    [property: JsonPropertyName("caption")] string? Caption
);

public record CustomScriptDto(
    [property: JsonPropertyName("trigger")] string Trigger,
    [property: JsonPropertyName("script")] string Script
);

public record PagePermissionsDto(
    [property: JsonPropertyName("editableBy")] List<string> EditableBy,
    [property: JsonPropertyName("viewableBy")] List<string> ViewableBy
);

// ================================================================================================
// VERSION HISTORY DTOs
// ================================================================================================

public record LandingPageVersionDto(
    [property: JsonPropertyName("versionId")] string VersionId,
    [property: JsonPropertyName("pageId")] string PageId,
    [property: JsonPropertyName("versionNumber")] int VersionNumber,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt,
    [property: JsonPropertyName("createdBy")] string? CreatedBy,
    [property: JsonPropertyName("changeDescription")] string? ChangeDescription
);

// ================================================================================================
// ANALYTICS DTOs
// ================================================================================================

public record LandingPageAnalyticsDto(
    [property: JsonPropertyName("pageId")] string PageId,
    [property: JsonPropertyName("totalViews")] long TotalViews,
    [property: JsonPropertyName("uniqueVisitors")] long UniqueVisitors,
    [property: JsonPropertyName("sectionViews")] Dictionary<string, long>? SectionViews,
    [property: JsonPropertyName("topReferrers")] Dictionary<string, long>? TopReferrers
);

public record TrackEventRequest(
    [property: JsonPropertyName("pageId")] string PageId,
    [property: JsonPropertyName("eventType")] string EventType,
    [property: JsonPropertyName("eventData")] Dictionary<string, object>? EventData,
    [property: JsonPropertyName("sessionId")] string? SessionId
);

