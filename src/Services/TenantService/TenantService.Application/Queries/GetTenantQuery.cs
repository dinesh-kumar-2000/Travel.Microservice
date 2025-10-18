using MediatR;
using TenantService.Contracts.DTOs;
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
        return tenant == null ? null : new TenantDto(tenant.Id, tenant.Name, tenant.Subdomain, 
            tenant.ContactEmail, tenant.Status.ToString(), tenant.Tier.ToString());
    }
}

