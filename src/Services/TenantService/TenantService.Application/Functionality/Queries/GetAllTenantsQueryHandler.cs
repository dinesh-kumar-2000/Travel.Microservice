using MediatR;
using TenantService.Application.DTOs;
using TenantService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace TenantService.Application.Queries;

public class GetAllTenantsQueryHandler : IRequestHandler<GetAllTenantsQuery, PagedTenantsResponse>
{
    private readonly ITenantRepository _repository;
    private readonly ILogger<GetAllTenantsQueryHandler> _logger;

    public GetAllTenantsQueryHandler(
        ITenantRepository repository,
        ILogger<GetAllTenantsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PagedTenantsResponse> Handle(GetAllTenantsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting tenants - Page: {Page}, PageSize: {PageSize}, Status: {Status}", 
            request.Page, request.PageSize, request.Status ?? "All");

        var (tenants, totalCount) = await _repository.GetPagedTenantsAsync(
            request.Page, 
            request.PageSize, 
            request.Status, 
            cancellationToken);

        var tenantDtos = tenants.Select(t =>
        {
            var settings = new TenantSettings(
                Theme: "light",
                PrimaryColor: t.Configuration.PrimaryColor,
                SecondaryColor: t.Configuration.SecondaryColor,
                LogoUrl: t.Configuration.LogoUrl,
                FaviconUrl: null,
                CustomCss: null
            );

            return new TenantDto(
                t.Id,
                t.Name,
                t.Subdomain,
                t.ContactEmail,
                t.Status.ToString(),
                t.Tier.ToString(),
                t.Configuration.LogoUrl,
                t.Configuration.PrimaryColor,
                t.Configuration.SecondaryColor,
                t.Status == Domain.Entities.TenantStatus.Active,
                settings
            );
        });

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        _logger.LogInformation("Retrieved {Count} tenants out of {TotalCount}", 
            tenants.Count(), totalCount);

        return new PagedTenantsResponse(
            tenantDtos,
            totalCount,
            request.Page,
            request.PageSize,
            totalPages
        );
    }
}

