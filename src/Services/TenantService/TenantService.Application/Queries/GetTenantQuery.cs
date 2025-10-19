using MediatR;
using TenantService.Contracts.DTOs;
using TenantService.Domain.Entities;
using TenantService.Domain.Repositories;

namespace TenantService.Application.Queries;

public record GetTenantByIdQuery(string TenantId) : IRequest<TenantDto?>;

public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, TenantDto?>
{
    private readonly ITenantRepository _repository;

    public GetTenantByIdQueryHandler(ITenantRepository repository)
    {
        _repository = repository;
    }

    public async Task<TenantDto?> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _repository.GetByIdAsync(request.TenantId, cancellationToken);
        
        if (tenant == null)
            return null;

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
            IsActive: tenant.Status == TenantStatus.Active,
            Settings: settings
        );
    }
}

