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
        return tenant == null ? null : new TenantDto(
            tenant.Id,
            tenant.Name,
            tenant.Name, // DisplayName - same as Name for now
            tenant.Subdomain,
            tenant.ContactEmail,
            tenant.Status.ToString(),
            tenant.Tier.ToString(),
            tenant.Configuration.LogoUrl,
            $"{tenant.Tier} tier travel services", // Description
            tenant.Status == TenantStatus.Active
        );
    }
}

