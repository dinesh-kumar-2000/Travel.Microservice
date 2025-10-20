using TenantService.Domain.Entities;

namespace TenantService.Domain.Repositories;

public interface ILandingPageRepository
{
    Task<LandingPage?> GetByIdAsync(string pageId, CancellationToken cancellationToken = default);
    Task<LandingPage?> GetBySlugAsync(string tenantId, string slug, string language = "en", CancellationToken cancellationToken = default);
    Task<IEnumerable<LandingPage>> GetByTenantIdAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<LandingPage>> GetPublishedByTenantIdAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<LandingPage>> SearchAsync(string tenantId, string? searchTerm, string? status, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string pageId, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string tenantId, string slug, string language, string? excludePageId = null, CancellationToken cancellationToken = default);
    Task<LandingPage> AddAsync(LandingPage page, CancellationToken cancellationToken = default);
    Task<LandingPage> UpdateAsync(LandingPage page, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string pageId, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(string tenantId, CancellationToken cancellationToken = default);
}

