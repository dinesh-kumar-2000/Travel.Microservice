using IdentityService.Domain.Entities;
using SharedKernel.Interfaces;

namespace IdentityService.Domain.Repositories;

public partial interface IUserRepository : IRepository<User, string>
{
    Task<User?> GetByEmailAsync(string email, string tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default);
    Task AssignRoleAsync(string userId, string roleId, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, string tenantId, CancellationToken cancellationToken = default);
}

