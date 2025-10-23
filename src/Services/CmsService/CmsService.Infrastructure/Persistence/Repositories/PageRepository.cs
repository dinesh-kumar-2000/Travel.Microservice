using CmsService.Domain.Entities;
using CmsService.Application.Interfaces;
using SharedKernel.Data;
using SharedKernel.Models;

namespace CmsService.Infrastructure.Persistence.Repositories;

public class PageRepository : BaseRepository<CmsPage>, IPageRepository
{
    public PageRepository(DapperContext context) : base(context)
    {
    }

    public async Task<CmsPage?> GetBySlugAsync(string slug)
    {
        const string sql = "SELECT * FROM cms_pages WHERE slug = @Slug AND is_deleted = false";
        return await QueryFirstOrDefaultAsync<CmsPage>(sql, new { Slug = slug });
    }

    public async Task<PaginatedResult<CmsPage>> GetPublishedPagesAsync(int page, int pageSize)
    {
        const string sql = @"
            SELECT * FROM cms_pages 
            WHERE is_published = true AND is_deleted = false 
            ORDER BY sort_order, title 
            LIMIT @PageSize OFFSET @Offset";
        
        const string countSql = "SELECT COUNT(*) FROM cms_pages WHERE is_published = true AND is_deleted = false";
        
        var offset = (page - 1) * pageSize;
        var items = await QueryAsync<CmsPage>(sql, new { PageSize = pageSize, Offset = offset });
        var totalCount = await QuerySingleAsync<int>(countSql);
        
        return new PaginatedResult<CmsPage>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<CmsPage>> GetChildPagesAsync(Guid parentPageId)
    {
        const string sql = @"
            SELECT * FROM cms_pages 
            WHERE parent_page_id = @ParentPageId AND is_deleted = false 
            ORDER BY sort_order, title";
        
        return await QueryAsync<CmsPage>(sql, new { ParentPageId = parentPageId });
    }

    public async Task<IEnumerable<CmsPage>> GetRootPagesAsync()
    {
        const string sql = @"
            SELECT * FROM cms_pages 
            WHERE parent_page_id IS NULL AND is_deleted = false 
            ORDER BY sort_order, title";
        
        return await QueryAsync<CmsPage>(sql);
    }
}
