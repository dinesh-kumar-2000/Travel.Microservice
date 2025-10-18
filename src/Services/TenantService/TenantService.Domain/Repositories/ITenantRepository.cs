using TenantService.Domain.Entities;
using SharedKernel.Interfaces;

namespace TenantService.Domain.Repositories;

public interface ITenantRepository : IRepository<Tenant, string>
{
    Task<Tenant?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default);
    Task<bool> SubdomainExistsAsync(string subdomain, CancellationToken cancellationToken = default);
}

