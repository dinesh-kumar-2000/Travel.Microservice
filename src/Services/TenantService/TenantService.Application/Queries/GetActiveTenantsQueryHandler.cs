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

        return tenants.Select(t => new TenantDto(
            t.Id,
            t.Name,
            t.Name, // DisplayName - can be enhanced later
            t.Subdomain,
            t.ContactEmail,
            t.Status.ToString(),
            t.Tier.ToString(),
            t.Configuration.LogoUrl,
            GenerateDescription(t),
            t.Status == TenantStatus.Active
        ));
    }

    private static string GenerateDescription(Tenant tenant)
    {
        return tenant.Tier switch
        {
            SubscriptionTier.Basic => "Basic travel services",
            SubscriptionTier.Standard => "Standard travel solutions",
            SubscriptionTier.Premium => "Premium travel experiences",
            SubscriptionTier.Enterprise => "Enterprise travel management",
            _ => "Travel services"
        };
    }
}

