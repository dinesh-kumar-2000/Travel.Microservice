using CmsService.Domain.Entities;
using SharedKernel.Data;

namespace CmsService.Domain.Repositories;

public interface IBlogPostRepository : ITenantBaseRepository<BlogPost, string>
{
    Task<List<BlogPost>> GetWithFiltersAsync(string tenantId, int page, int limit, string? search, string? status);
    Task<int> GetCountWithFiltersAsync(string tenantId, string? search, string? status);
    Task<BlogPost?> GetBySlugAsync(string slug, string tenantId);
}

