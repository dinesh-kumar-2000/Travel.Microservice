using TenantService.Domain.Entities;
using SharedKernel.Interfaces;

namespace TenantService.Application.Interfaces;

public interface ITenantConfigurationRepository : IRepository<TenantConfiguration, string>
{
    Task<TenantConfiguration?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> ExistsForTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
