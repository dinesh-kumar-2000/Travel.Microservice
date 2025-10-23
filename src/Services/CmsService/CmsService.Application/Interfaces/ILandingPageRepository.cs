using CmsService.Domain.Entities;
using SharedKernel.Data;

namespace CmsService.Application.Interfaces;

public interface ILandingPageRepository : ITenantBaseRepository<LandingPage, string>
{
    Task<LandingPage?> GetBySlugAsync(string tenantId, string slug, string language = "en", CancellationToken cancellationToken = default);
    Task<IEnumerable<LandingPage>> GetPublishedByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<LandingPage>> SearchAsync(string tenantId, string? searchTerm, string? status, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string tenantId, string slug, string language, string? excludePageId = null, CancellationToken cancellationToken = default);
}

