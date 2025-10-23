using IdentityService.Domain.Entities;
using SharedKernel.Interfaces;

namespace IdentityService.Domain.Interfaces;

public interface IRoleRepository : IRepository<Role, string>
{
    Task<Role?> GetByNameAsync(string name, string tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Role>> GetByUserIdAsync(Guid userId, string tenantId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, string tenantId, CancellationToken cancellationToken = default);
}
