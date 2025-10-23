using CmsService.Domain.Entities;
using CmsService.Application.Interfaces;
using SharedKernel.Data;
using SharedKernel.Models;

namespace CmsService.Infrastructure.Persistence.Repositories;

public class ContentRepository : BaseRepository<CmsContent>, IContentRepository
{
    public ContentRepository(DapperContext context) : base(context)
    {
    }

    public async Task<CmsContent?> GetBySlugAsync(string slug)
    {
        const string sql = "SELECT * FROM cms_content WHERE slug = @Slug AND is_deleted = false";
        return await QueryFirstOrDefaultAsync<CmsContent>(sql, new { Slug = slug });
    }

    public async Task<PaginatedResult<CmsContent>> GetPublishedContentAsync(int page, int pageSize)
    {
        const string sql = @"
            SELECT * FROM cms_content 
            WHERE is_published = true AND is_deleted = false 
            ORDER BY published_at DESC 
            LIMIT @PageSize OFFSET @Offset";
        
        const string countSql = "SELECT COUNT(*) FROM cms_content WHERE is_published = true AND is_deleted = false";
        
        var offset = (page - 1) * pageSize;
        var items = await QueryAsync<CmsContent>(sql, new { PageSize = pageSize, Offset = offset });
        var totalCount = await QuerySingleAsync<int>(countSql);
        
        return new PaginatedResult<CmsContent>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<CmsContent>> SearchContentAsync(string searchTerm)
    {
        const string sql = @"
            SELECT * FROM cms_content 
            WHERE (title ILIKE @SearchTerm OR content ILIKE @SearchTerm OR meta_keywords ILIKE @SearchTerm) 
            AND is_deleted = false 
            ORDER BY title";
        
        var searchPattern = $"%{searchTerm}%";
        return await QueryAsync<CmsContent>(sql, new { SearchTerm = searchPattern });
    }
}
