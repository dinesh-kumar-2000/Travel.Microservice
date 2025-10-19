using MediatR;
using Microsoft.Extensions.Logging;
using TenantService.Contracts.DTOs;
using TenantService.Domain.Repositories;

namespace TenantService.Application.Queries;

public record GetTenantBySubdomainQuery(string Subdomain) : IRequest<TenantDto?>;

public class GetTenantBySubdomainQueryHandler : IRequestHandler<GetTenantBySubdomainQuery, TenantDto?>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<GetTenantBySubdomainQueryHandler> _logger;

    public GetTenantBySubdomainQueryHandler(
        ITenantRepository tenantRepository,
        ILogger<GetTenantBySubdomainQueryHandler> logger)
    {
        _tenantRepository = tenantRepository;
        _logger = logger;
    }

    public async Task<TenantDto?> Handle(GetTenantBySubdomainQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting tenant by subdomain: {Subdomain}", request.Subdomain);

        var tenant = await _tenantRepository.GetBySubdomainAsync(request.Subdomain, cancellationToken);

        if (tenant == null)
        {
            _logger.LogWarning("Tenant with subdomain {Subdomain} not found", request.Subdomain);
            return null;
        }

        var settings = new TenantSettings(
            Theme: "light",
            PrimaryColor: tenant.Configuration.PrimaryColor,
            SecondaryColor: tenant.Configuration.SecondaryColor,
            LogoUrl: tenant.Configuration.LogoUrl,
            FaviconUrl: null,
            CustomCss: null
        );

        return new TenantDto(
            Id: tenant.Id,
            Name: tenant.Name,
            Subdomain: tenant.Subdomain,
            ContactEmail: tenant.ContactEmail,
            Status: tenant.Status.ToString(),
            Tier: tenant.Tier.ToString(),
            Logo: tenant.Configuration.LogoUrl,
            PrimaryColor: tenant.Configuration.PrimaryColor,
            SecondaryColor: tenant.Configuration.SecondaryColor,
            IsActive: tenant.Status == TenantService.Domain.Entities.TenantStatus.Active,
            Settings: settings
        );
    }
}

