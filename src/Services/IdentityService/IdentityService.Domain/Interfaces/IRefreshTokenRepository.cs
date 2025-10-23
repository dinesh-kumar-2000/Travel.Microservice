using IdentityService.Domain.Entities;
using SharedKernel.Interfaces;

namespace IdentityService.Domain.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken, string>
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
}
