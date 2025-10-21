using TenantService.Domain.Entities;
using SharedKernel.Interfaces;

namespace TenantService.Domain.Repositories;

public interface ITenantRepository : IRepository<Tenant, string>
{
    Task<Tenant?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default);
    Task<bool> SubdomainExistsAsync(string subdomain, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tenant>> GetActiveTenantsAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<Tenant> Tenants, int TotalCount)> GetPagedTenantsAsync(
        int page, 
        int pageSize, 
        string? status = null, 
        CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetTenantStatisticsAsync(CancellationToken cancellationToken = default);
}

