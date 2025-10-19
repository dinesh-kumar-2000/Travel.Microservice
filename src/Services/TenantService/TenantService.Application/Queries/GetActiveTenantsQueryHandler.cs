using MediatR;
using TenantService.Contracts.DTOs;
using TenantService.Domain.Entities;
using TenantService.Domain.Repositories;

namespace TenantService.Application.Queries;

public class GetActiveTenantsQueryHandler : IRequestHandler<GetActiveTenantsQuery, IEnumerable<TenantDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetActiveTenantsQueryHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<IEnumerable<TenantDto>> Handle(GetActiveTenantsQuery request, CancellationToken cancellationToken)
    {
        var tenants = await _tenantRepository.GetActiveTenantsAsync(cancellationToken);

        return tenants.Select(t =>
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
                Id: t.Id,
                Name: t.Name,
                Subdomain: t.Subdomain,
                ContactEmail: t.ContactEmail,
                Status: t.Status.ToString(),
                Tier: t.Tier.ToString(),
                Logo: t.Configuration.LogoUrl,
                PrimaryColor: t.Configuration.PrimaryColor,
                SecondaryColor: t.Configuration.SecondaryColor,
                IsActive: t.Status == TenantStatus.Active,
                Settings: settings
            );
        });
    }
}

