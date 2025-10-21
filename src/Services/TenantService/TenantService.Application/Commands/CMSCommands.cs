using TenantService.Contracts.DTOs;
using MediatR;

namespace TenantService.Application.Commands;

#region Blog Commands

public record CreateBlogPostCommand(
    Guid TenantId,
    Guid AuthorId,
    string AuthorName,
    string Title,
    string Slug,
    string Content,
    string? Excerpt,
    string? Category,
    List<string>? Tags,
    string? FeaturedImage,
    string Status
) : IRequest<BlogPostDto>;

public record UpdateBlogPostCommand(
    Guid Id,
    Guid TenantId,
    string Title,
    string Slug,
    string Content,
    string? Excerpt,
    string? Category,
    List<string>? Tags,
    string? FeaturedImage,
    string Status
) : IRequest<BlogPostDto>;

public record DeleteBlogPostCommand(
    Guid Id,
    Guid TenantId
) : IRequest<bool>;

#endregion

#region FAQ Commands

public record CreateFAQCommand(
    Guid TenantId,
    string Question,
    string Answer,
    string Category,
    int DisplayOrder,
    bool IsActive
) : IRequest<FAQDto>;

public record UpdateFAQCommand(
    Guid Id,
    Guid TenantId,
    string Question,
    string Answer,
    string Category,
    int DisplayOrder,
    bool IsActive
) : IRequest<FAQDto>;

public record DeleteFAQCommand(
    Guid Id,
    Guid TenantId
) : IRequest<bool>;

#endregion

#region CMS Page Commands

public record CreateCMSPageCommand(
    Guid TenantId,
    string Title,
    string Slug,
    string Content,
    string PageType,
    string Status,
    string? MetaTitle,
    string? MetaDescription
) : IRequest<CMSPageDto>;

public record UpdateCMSPageCommand(
    Guid Id,
    Guid TenantId,
    string Title,
    string Slug,
    string Content,
    string PageType,
    string Status,
    string? MetaTitle,
    string? MetaDescription
) : IRequest<CMSPageDto>;

public record DeleteCMSPageCommand(
    Guid Id,
    Guid TenantId
) : IRequest<bool>;

#endregion

#region Template Commands

public record CreateEmailTemplateCommand(
    Guid TenantId,
    string Name,
    string TemplateType,
    string? Category,
    string Subject,
    string Content,
    List<string> Variables,
    bool IsActive
) : IRequest<EmailTemplateDto>;

public record CreateSMSTemplateCommand(
    Guid TenantId,
    string Name,
    string TemplateType,
    string? Category,
    string Content,
    List<string> Variables,
    bool IsActive
) : IRequest<SMSTemplateDto>;

public record UpdateTemplateCommand(
    Guid Id,
    Guid TenantId,
    string Name,
    string? Subject,
    string Content,
    List<string> Variables,
    bool IsActive
) : IRequest<TemplateDto>;

public record DeleteTemplateCommand(
    Guid Id,
    Guid TenantId
) : IRequest<bool>;

#endregion

#region SEO Commands

public record UpdateSEOSettingsCommand(
    Guid TenantId,
    GeneralSEODto General,
    OpenGraphDto OpenGraph,
    TwitterCardDto Twitter,
    TechnicalSEODto Technical,
    SitemapDto Sitemap,
    SchemaDto Schema
) : IRequest<SEOSettingsDto>;

public record GenerateSitemapCommand(
    Guid TenantId
) : IRequest<SitemapResponseDto>;

#endregion

