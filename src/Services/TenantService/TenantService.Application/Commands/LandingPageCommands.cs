using MediatR;
using TenantService.Contracts.DTOs;
using TenantService.Domain.Entities;
using TenantService.Domain.Repositories;
using SharedKernel.Utilities;
using SharedKernel.Exceptions;

namespace TenantService.Application.Commands;

// ================================================================================================
// CREATE LANDING PAGE
// ================================================================================================

public record CreateLandingPageCommand(CreateLandingPageRequest Request, string? CreatedBy = null) : IRequest<LandingPageDto>;

public class CreateLandingPageCommandHandler : IRequestHandler<CreateLandingPageCommand, LandingPageDto>
{
    private readonly ILandingPageRepository _repository;
    private readonly ITenantRepository _tenantRepository;

    public CreateLandingPageCommandHandler(ILandingPageRepository repository, ITenantRepository tenantRepository)
    {
        _repository = repository;
        _tenantRepository = tenantRepository;
    }

    public async Task<LandingPageDto> Handle(CreateLandingPageCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        
        // Validate tenant exists
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant == null)
            throw new NotFoundException("Tenant", request.TenantId);

        // Check if slug already exists for this tenant
        if (await _repository.SlugExistsAsync(request.TenantId, request.Slug, request.Language, null, cancellationToken))
            throw new ValidationException("Slug", $"Slug '{request.Slug}' already exists for this tenant and language");

        // Create landing page entity
        var page = LandingPage.Create(
            request.PageId,
            request.TenantId,
            request.PageName,
            request.Slug,
            request.Language,
            command.CreatedBy
        );

        // Set configurations
        if (request.Seo != null)
        {
            page.UpdateSeo(MapSeoConfig(request.Seo), command.CreatedBy);
        }

        if (request.Theme != null)
        {
            page.UpdateTheme(MapThemeConfig(request.Theme), command.CreatedBy);
        }

        if (request.Layout != null)
        {
            page.UpdateLayout(MapLayoutConfig(request.Layout), command.CreatedBy);
        }

        if (request.Sections != null && request.Sections.Any())
        {
            page.SetSections(MapSections(request.Sections), command.CreatedBy);
        }

        if (request.MediaLibrary != null && request.MediaLibrary.Any())
        {
            page.UpdateMediaLibrary(MapMediaAssets(request.MediaLibrary), command.CreatedBy);
        }

        if (request.CustomScripts != null && request.CustomScripts.Any())
        {
            page.UpdateCustomScripts(MapCustomScripts(request.CustomScripts), command.CreatedBy);
        }

        if (request.Permissions != null)
        {
            typeof(LandingPage).GetProperty("Permissions")!.SetValue(page, MapPermissions(request.Permissions));
        }

        await _repository.AddAsync(page, cancellationToken);

        return MapToDto(page);
    }

    internal static SeoConfiguration MapSeoConfig(SeoConfigDto dto)
    {
        return new SeoConfiguration
        {
            Title = dto.Title,
            Description = dto.Description,
            Keywords = dto.Keywords ?? new List<string>(),
            CanonicalUrl = dto.CanonicalUrl,
            OpenGraph = dto.OpenGraph != null ? new OpenGraphConfig
            {
                OgTitle = dto.OpenGraph.OgTitle,
                OgDescription = dto.OpenGraph.OgDescription,
                OgImage = dto.OpenGraph.OgImage,
                OgType = dto.OpenGraph.OgType
            } : null
        };
    }

    internal static ThemeConfiguration MapThemeConfig(ThemeConfigDto dto)
    {
        return new ThemeConfiguration
        {
            PrimaryColor = dto.PrimaryColor,
            SecondaryColor = dto.SecondaryColor,
            FontFamily = dto.FontFamily,
            BackgroundColor = dto.BackgroundColor,
            TextColor = dto.TextColor,
            Button = dto.Button != null ? new ButtonStyle
            {
                Style = dto.Button.Style,
                HoverColor = dto.Button.HoverColor
            } : null
        };
    }

    internal static LayoutConfiguration MapLayoutConfig(LayoutConfigDto dto)
    {
        return new LayoutConfiguration
        {
            ContainerWidth = dto.ContainerWidth,
            GridSystem = dto.GridSystem,
            Spacing = dto.Spacing != null ? new SpacingConfig
            {
                SectionPadding = dto.Spacing.SectionPadding,
                ComponentGap = dto.Spacing.ComponentGap
            } : null,
            Breakpoints = dto.Breakpoints != null ? new BreakpointConfig
            {
                Mobile = dto.Breakpoints.Mobile,
                Tablet = dto.Breakpoints.Tablet,
                Desktop = dto.Breakpoints.Desktop
            } : null
        };
    }

    internal static List<PageSection> MapSections(List<PageSectionDto> dtos)
    {
        return dtos.Select(dto => new PageSection
        {
            SectionId = dto.SectionId,
            Type = dto.Type,
            Order = dto.Order,
            Visible = dto.Visible,
            Content = dto.Content ?? new Dictionary<string, object>(),
            Background = dto.Background,
            Animation = dto.Animation,
            AdditionalProps = BuildAdditionalProps(dto)
        }).ToList();
    }

    private static Dictionary<string, object> BuildAdditionalProps(PageSectionDto dto)
    {
        var props = new Dictionary<string, object>();
        
        if (!string.IsNullOrEmpty(dto.Title))
            props["title"] = dto.Title;
        
        if (dto.Columns.HasValue)
            props["columns"] = dto.Columns.Value;
        
        if (dto.Cards != null)
            props["cards"] = dto.Cards;
        
        if (dto.Items != null)
            props["items"] = dto.Items;
        
        return props;
    }

    internal static List<MediaAsset> MapMediaAssets(List<MediaAssetDto> dtos)
    {
        return dtos.Select(dto => new MediaAsset
        {
            Id = dto.Id,
            Url = dto.Url,
            Type = dto.Type,
            AltText = dto.AltText,
            Caption = dto.Caption
        }).ToList();
    }

    internal static List<CustomScript> MapCustomScripts(List<CustomScriptDto> dtos)
    {
        return dtos.Select(dto => new CustomScript
        {
            Trigger = dto.Trigger,
            Script = dto.Script
        }).ToList();
    }

    private static PagePermissions MapPermissions(PagePermissionsDto dto)
    {
        return new PagePermissions
        {
            EditableBy = dto.EditableBy,
            ViewableBy = dto.ViewableBy
        };
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
                OpenGraph: page.Seo.OpenGraph != null ? new OpenGraphDto(
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
                Animation: s.Animation
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
// UPDATE LANDING PAGE
// ================================================================================================

public record UpdateLandingPageCommand(string PageId, UpdateLandingPageRequest Request, string? ModifiedBy = null) : IRequest<LandingPageDto>;

public class UpdateLandingPageCommandHandler : IRequestHandler<UpdateLandingPageCommand, LandingPageDto>
{
    private readonly ILandingPageRepository _repository;

    public UpdateLandingPageCommandHandler(ILandingPageRepository repository)
    {
        _repository = repository;
    }

    public async Task<LandingPageDto> Handle(UpdateLandingPageCommand command, CancellationToken cancellationToken)
    {
        var page = await _repository.GetByIdAsync(command.PageId, cancellationToken);
        if (page == null)
            throw new NotFoundException("LandingPage", command.PageId);

        var request = command.Request;

        // Check if slug is being changed and if it already exists
        if (page.Slug != request.Slug)
        {
            if (await _repository.SlugExistsAsync(page.TenantId, request.Slug, request.Language, command.PageId, cancellationToken))
                throw new ValidationException("Slug", $"Slug '{request.Slug}' already exists for this tenant and language");
        }

        // Update basic info
        page.UpdateBasicInfo(request.PageName, request.Slug, request.Language, command.ModifiedBy);

        // Update configurations
        if (request.Seo != null)
        {
            page.UpdateSeo(CreateLandingPageCommandHandler.MapSeoConfig(request.Seo), command.ModifiedBy);
        }

        if (request.Theme != null)
        {
            page.UpdateTheme(CreateLandingPageCommandHandler.MapThemeConfig(request.Theme), command.ModifiedBy);
        }

        if (request.Layout != null)
        {
            page.UpdateLayout(CreateLandingPageCommandHandler.MapLayoutConfig(request.Layout), command.ModifiedBy);
        }

        if (request.Sections != null)
        {
            page.SetSections(CreateLandingPageCommandHandler.MapSections(request.Sections), command.ModifiedBy);
        }

        if (request.MediaLibrary != null)
        {
            page.UpdateMediaLibrary(CreateLandingPageCommandHandler.MapMediaAssets(request.MediaLibrary), command.ModifiedBy);
        }

        if (request.CustomScripts != null)
        {
            page.UpdateCustomScripts(CreateLandingPageCommandHandler.MapCustomScripts(request.CustomScripts), command.ModifiedBy);
        }

        await _repository.UpdateAsync(page, cancellationToken);

        return CreateLandingPageCommandHandler.MapToDto(page);
    }
}

// ================================================================================================
// DELETE LANDING PAGE
// ================================================================================================

public record DeleteLandingPageCommand(string PageId) : IRequest<bool>;

public class DeleteLandingPageCommandHandler : IRequestHandler<DeleteLandingPageCommand, bool>
{
    private readonly ILandingPageRepository _repository;

    public DeleteLandingPageCommandHandler(ILandingPageRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteLandingPageCommand command, CancellationToken cancellationToken)
    {
        var exists = await _repository.ExistsAsync(command.PageId, cancellationToken);
        if (!exists)
            throw new NotFoundException("LandingPage", command.PageId);

        return await _repository.DeleteAsync(command.PageId, cancellationToken);
    }
}

// ================================================================================================
// PUBLISH/UNPUBLISH LANDING PAGE
// ================================================================================================

public record PublishLandingPageCommand(string PageId, string? ModifiedBy = null) : IRequest<LandingPageDto>;

public class PublishLandingPageCommandHandler : IRequestHandler<PublishLandingPageCommand, LandingPageDto>
{
    private readonly ILandingPageRepository _repository;

    public PublishLandingPageCommandHandler(ILandingPageRepository repository)
    {
        _repository = repository;
    }

    public async Task<LandingPageDto> Handle(PublishLandingPageCommand command, CancellationToken cancellationToken)
    {
        var page = await _repository.GetByIdAsync(command.PageId, cancellationToken);
        if (page == null)
            throw new NotFoundException("LandingPage", command.PageId);

        page.Publish(command.ModifiedBy);
        await _repository.UpdateAsync(page, cancellationToken);

        return CreateLandingPageCommandHandler.MapToDto(page);
    }
}

public record UnpublishLandingPageCommand(string PageId, string? ModifiedBy = null) : IRequest<LandingPageDto>;

public class UnpublishLandingPageCommandHandler : IRequestHandler<UnpublishLandingPageCommand, LandingPageDto>
{
    private readonly ILandingPageRepository _repository;

    public UnpublishLandingPageCommandHandler(ILandingPageRepository repository)
    {
        _repository = repository;
    }

    public async Task<LandingPageDto> Handle(UnpublishLandingPageCommand command, CancellationToken cancellationToken)
    {
        var page = await _repository.GetByIdAsync(command.PageId, cancellationToken);
        if (page == null)
            throw new NotFoundException("LandingPage", command.PageId);

        page.Unpublish(command.ModifiedBy);
        await _repository.UpdateAsync(page, cancellationToken);

        return CreateLandingPageCommandHandler.MapToDto(page);
    }
}

public record ArchiveLandingPageCommand(string PageId, string? ModifiedBy = null) : IRequest<LandingPageDto>;

public class ArchiveLandingPageCommandHandler : IRequestHandler<ArchiveLandingPageCommand, LandingPageDto>
{
    private readonly ILandingPageRepository _repository;

    public ArchiveLandingPageCommandHandler(ILandingPageRepository repository)
    {
        _repository = repository;
    }

    public async Task<LandingPageDto> Handle(ArchiveLandingPageCommand command, CancellationToken cancellationToken)
    {
        var page = await _repository.GetByIdAsync(command.PageId, cancellationToken);
        if (page == null)
            throw new NotFoundException("LandingPage", command.PageId);

        page.Archive(command.ModifiedBy);
        await _repository.UpdateAsync(page, cancellationToken);

        return CreateLandingPageCommandHandler.MapToDto(page);
    }
}

