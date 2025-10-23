using CmsService.Domain.Entities;
using SharedKernel.Data;
using SharedKernel.Models;

namespace CmsService.Application.Interfaces;

public interface IPageRepository : IBaseRepository<CmsPage, string>
{
    Task<CmsPage?> GetBySlugAsync(string slug);
    Task<PagedResult<CmsPage>> GetPublishedPagesAsync(int page, int pageSize);
    Task<IEnumerable<CmsPage>> GetChildPagesAsync(string parentPageId);
    Task<IEnumerable<CmsPage>> GetRootPagesAsync();
}
