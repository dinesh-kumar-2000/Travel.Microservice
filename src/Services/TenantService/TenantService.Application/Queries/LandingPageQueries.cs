using MediatR;
using TenantService.Contracts.DTOs;
using TenantService.Domain.Entities;
using TenantService.Domain.Repositories;

namespace TenantService.Application.Queries;

// ================================================================================================
// GET LANDING PAGE BY ID
// ================================================================================================

public record GetLandingPageByIdQuery(string PageId) : IRequest<LandingPageDto?>;

public class GetLandingPageByIdQueryHandler : IRequestHandler<GetLandingPageByIdQuery, LandingPageDto?>
{
    private readonly ILandingPageRepository _repository;

    public GetLandingPageByIdQueryHandler(ILandingPageRepository repository)
    {
        _repository = repository;
    }

    public async Task<LandingPageDto?> Handle(GetLandingPageByIdQuery request, CancellationToken cancellationToken)
    {
        var page = await _repository.GetByIdAsync(request.PageId, cancellationToken);
        return page != null ? MapToDto(page) : null;
    }

    internal static LandingPageDto MapToDto(LandingPage page)
    {
        return new LandingPageDto(
            PageId: page.Id,
            TenantId: page.TenantId,
            PageName: page.PageName,
            Slug: page.Slug,
            Status: page.Status.ToString(),
            Language: page.Language,
            Seo: page.Seo != null ? new SeoConfigDto(
                Title: page.Seo.Title,
                Description: page.Seo.Description,
                Keywords: page.Seo.Keywords,
                CanonicalUrl: page.Seo.CanonicalUrl,
                OpenGraph: page.Seo.OpenGraph != null ? new LandingPageOpenGraphDto(
                    OgTitle: page.Seo.OpenGraph.OgTitle,
                    OgDescription: page.Seo.OpenGraph.OgDescription,
                    OgImage: page.Seo.OpenGraph.OgImage,
                    OgType: page.Seo.OpenGraph.OgType
                ) : null
            ) : null,
            Theme: page.Theme != null ? new ThemeConfigDto(
                PrimaryColor: page.Theme.PrimaryColor,
                SecondaryColor: page.Theme.SecondaryColor,
                FontFamily: page.Theme.FontFamily,
                BackgroundColor: page.Theme.BackgroundColor,
                TextColor: page.Theme.TextColor,
                Button: page.Theme.Button != null ? new ButtonStyleDto(
                    Style: page.Theme.Button.Style,
                    HoverColor: page.Theme.Button.HoverColor
                ) : null
            ) : null,
            Layout: page.Layout != null ? new LayoutConfigDto(
                ContainerWidth: page.Layout.ContainerWidth,
                GridSystem: page.Layout.GridSystem,
                Spacing: page.Layout.Spacing != null ? new SpacingDto(
                    SectionPadding: page.Layout.Spacing.SectionPadding,
                    ComponentGap: page.Layout.Spacing.ComponentGap
                ) : null,
                Breakpoints: page.Layout.Breakpoints != null ? new BreakpointDto(
                    Mobile: page.Layout.Breakpoints.Mobile,
                    Tablet: page.Layout.Breakpoints.Tablet,
                    Desktop: page.Layout.Breakpoints.Desktop
                ) : null
            ) : null,
            Sections: page.Sections.Select(s => new PageSectionDto(
                SectionId: s.SectionId,
                Type: s.Type,
                Order: s.Order,
                Visible: s.Visible,
                Content: s.Content,
                Background: s.Background,
                Animation: s.Animation,
                Title: s.AdditionalProps?.ContainsKey("title") == true ? s.AdditionalProps["title"]?.ToString() : null,
                Columns: s.AdditionalProps?.ContainsKey("columns") == true ? Convert.ToInt32(s.AdditionalProps["columns"]) : null,
                Cards: s.AdditionalProps?.ContainsKey("cards") == true ? s.AdditionalProps["cards"] as List<Dictionary<string, object>> : null,
                Items: s.AdditionalProps?.ContainsKey("items") == true ? s.AdditionalProps["items"] as List<Dictionary<string, object>> : null
            )).ToList(),
            MediaLibrary: page.MediaLibrary.Select(m => new MediaAssetDto(
                Id: m.Id,
                Url: m.Url,
                Type: m.Type,
                AltText: m.AltText,
                Caption: m.Caption
            )).ToList(),
            CustomScripts: page.CustomScripts.Select(cs => new CustomScriptDto(
                Trigger: cs.Trigger,
                Script: cs.Script
            )).ToList(),
            Permissions: new PagePermissionsDto(
                EditableBy: page.Permissions.EditableBy,
                ViewableBy: page.Permissions.ViewableBy
            ),
            LastModified: page.LastModified,
            PublishedAt: page.PublishedAt,
            Version: page.Version,
            CreatedAt: page.CreatedAt,
            CreatedBy: page.CreatedBy
        );
    }
}

// ================================================================================================
// GET LANDING PAGE BY SLUG
// ================================================================================================

public record GetLandingPageBySlugQuery(string TenantId, string Slug, string Language = "en") : IRequest<LandingPageDto?>;

public class GetLandingPageBySlugQueryHandler : IRequestHandler<GetLandingPageBySlugQuery, LandingPageDto?>
{
    private readonly ILandingPageRepository _repository;

    public GetLandingPageBySlugQueryHandler(ILandingPageRepository repository)
    {
        _repository = repository;
    }

    public async Task<LandingPageDto?> Handle(GetLandingPageBySlugQuery request, CancellationToken cancellationToken)
    {
        var page = await _repository.GetBySlugAsync(request.TenantId, request.Slug, request.Language, cancellationToken);
        return page != null ? GetLandingPageByIdQueryHandler.MapToDto(page) : null;
    }
}

// ================================================================================================
// GET LANDING PAGE BY SUBDOMAIN AND SLUG
// ================================================================================================

public record GetLandingPageBySubdomainQuery(string Subdomain, string Slug, string Language = "en") : IRequest<LandingPageDto?>;

public class GetLandingPageBySubdomainQueryHandler : IRequestHandler<GetLandingPageBySubdomainQuery, LandingPageDto?>
{
    private readonly ILandingPageRepository _landingPageRepository;
    private readonly ITenantRepository _tenantRepository;

    public GetLandingPageBySubdomainQueryHandler(
        ILandingPageRepository landingPageRepository,
        ITenantRepository tenantRepository)
    {
        _landingPageRepository = landingPageRepository;
        _tenantRepository = tenantRepository;
    }

    public async Task<LandingPageDto?> Handle(GetLandingPageBySubdomainQuery request, CancellationToken cancellationToken)
    {
        // First, lookup tenant by subdomain
        var tenant = await _tenantRepository.GetBySubdomainAsync(request.Subdomain, cancellationToken);
        if (tenant == null)
            return null;

        // Then get the landing page for that tenant
        var page = await _landingPageRepository.GetBySlugAsync(tenant.Id, request.Slug, request.Language, cancellationToken);
        return page != null ? GetLandingPageByIdQueryHandler.MapToDto(page) : null;
    }
}

// ================================================================================================
// GET LANDING PAGES BY TENANT
// ================================================================================================

public record GetLandingPagesByTenantQuery(string TenantId, bool PublishedOnly = false) : IRequest<IEnumerable<LandingPageSummaryDto>>;

public class GetLandingPagesByTenantQueryHandler : IRequestHandler<GetLandingPagesByTenantQuery, IEnumerable<LandingPageSummaryDto>>
{
    private readonly ILandingPageRepository _repository;

    public GetLandingPagesByTenantQueryHandler(ILandingPageRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<LandingPageSummaryDto>> Handle(GetLandingPagesByTenantQuery request, CancellationToken cancellationToken)
    {
        var pages = request.PublishedOnly
            ? await _repository.GetPublishedByTenantIdAsync(request.TenantId, cancellationToken)
            : await _repository.GetByTenantIdAsync(request.TenantId, cancellationToken);

        return pages.Select(p => new LandingPageSummaryDto(
            PageId: p.Id,
            TenantId: p.TenantId,
            PageName: p.PageName,
            Slug: p.Slug,
            Status: p.Status.ToString(),
            Language: p.Language,
            LastModified: p.LastModified,
            PublishedAt: p.PublishedAt,
            Version: p.Version
        ));
    }
}

// ================================================================================================
// SEARCH LANDING PAGES
// ================================================================================================

public record SearchLandingPagesQuery(
    string TenantId,
    string? SearchTerm = null,
    string? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<SearchLandingPagesResult>;

public record SearchLandingPagesResult(
    IEnumerable<LandingPageSummaryDto> Pages,
    int TotalCount,
    int Page,
    int PageSize
);

public class SearchLandingPagesQueryHandler : IRequestHandler<SearchLandingPagesQuery, SearchLandingPagesResult>
{
    private readonly ILandingPageRepository _repository;

    public SearchLandingPagesQueryHandler(ILandingPageRepository repository)
    {
        _repository = repository;
    }

    public async Task<SearchLandingPagesResult> Handle(SearchLandingPagesQuery request, CancellationToken cancellationToken)
    {
        var pages = await _repository.SearchAsync(
            request.TenantId,
            request.SearchTerm,
            request.Status,
            request.Page,
            request.PageSize,
            cancellationToken
        );

        var totalCount = await _repository.GetTotalCountAsync(request.TenantId, cancellationToken);

        var summaries = pages.Select(p => new LandingPageSummaryDto(
            PageId: p.Id,
            TenantId: p.TenantId,
            PageName: p.PageName,
            Slug: p.Slug,
            Status: p.Status.ToString(),
            Language: p.Language,
            LastModified: p.LastModified,
            PublishedAt: p.PublishedAt,
            Version: p.Version
        ));

        return new SearchLandingPagesResult(
            Pages: summaries,
            TotalCount: totalCount,
            Page: request.Page,
            PageSize: request.PageSize
        );
    }
}

