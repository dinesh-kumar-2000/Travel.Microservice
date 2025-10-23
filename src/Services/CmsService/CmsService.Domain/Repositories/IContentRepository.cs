using CmsService.Domain.Entities;
using SharedKernel.Data;
using SharedKernel.Models;

namespace CmsService.Domain.Repositories;

public interface IContentRepository : IBaseRepository<CmsContent, string>
{
    Task<CmsContent?> GetBySlugAsync(string slug);
    Task<PagedResult<CmsContent>> GetPublishedContentAsync(int page, int pageSize);
    Task<IEnumerable<CmsContent>> SearchContentAsync(string searchTerm);
}
