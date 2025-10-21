using TenantService.Contracts.DTOs;
using MediatR;

namespace TenantService.Application.Queries;

#region Blog Queries

public record GetBlogPostsQuery(
    Guid TenantId,
    int Page,
    int Limit,
    string? Search,
    string? Status
) : IRequest<PagedResult<BlogPostDto>>;

public record GetBlogPostByIdQuery(
    Guid Id,
    Guid TenantId
) : IRequest<BlogPostDto?>;

public record GetBlogPostBySlugQuery(
    string Slug,
    Guid TenantId
) : IRequest<BlogPostDto?>;

#endregion

#region FAQ Queries

public record GetFAQsQuery(
    Guid TenantId,
    string? Category
) : IRequest<List<FAQDto>>;

#endregion

#region CMS Page Queries

public record GetCMSPagesQuery(
    Guid TenantId,
    string? PageType
) : IRequest<List<CMSPageDto>>;

public record GetCMSPageByIdQuery(
    Guid Id,
    Guid TenantId
) : IRequest<CMSPageDto?>;

public record GetCMSPageBySlugQuery(
    string Slug,
    Guid TenantId
) : IRequest<CMSPageDto?>;

#endregion

#region Template Queries

public record GetTemplatesQuery(
    Guid TenantId,
    string? Type
) : IRequest<TemplatesResponseDto>;

public record GetTemplateByIdQuery(
    Guid Id,
    Guid TenantId
) : IRequest<TemplateDto?>;

#endregion

#region SEO Queries

public record GetSEOSettingsQuery(
    Guid TenantId
) : IRequest<SEOSettingsDto?>;

#endregion

