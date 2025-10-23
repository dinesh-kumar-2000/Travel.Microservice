using TenantService.Domain.Entities;
using SharedKernel.Interfaces;

namespace TenantService.Application.Interfaces;

public interface ITenantAdminRepository : IRepository<TenantAdmin, string>
{
    Task<IEnumerable<TenantAdmin>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<TenantAdmin?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsUserAdminAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TenantAdmin>> GetActiveAdminsAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
