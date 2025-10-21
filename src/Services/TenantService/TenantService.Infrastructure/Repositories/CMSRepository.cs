using Dapper;
using TenantService.Domain.Entities;
using Microsoft.Extensions.Logging;
using Npgsql;
using Tenancy;

namespace TenantService.Infrastructure.Repositories;

public interface ICMSRepository
{
    // Blog Posts
    Task<BlogPost?> GetBlogPostByIdAsync(Guid id, Guid tenantId);
    Task<List<BlogPost>> GetBlogPostsAsync(Guid tenantId, int page, int limit, string? search, string? status);
    Task<int> GetBlogPostCountAsync(Guid tenantId, string? search, string? status);
    Task<BlogPost> CreateBlogPostAsync(BlogPost post);
    Task<BlogPost?> UpdateBlogPostAsync(BlogPost post);
    Task<bool> DeleteBlogPostAsync(Guid id, Guid tenantId);

    // FAQs
    Task<List<FAQ>> GetFAQsAsync(Guid tenantId, string? category);
    Task<FAQ?> GetFAQByIdAsync(Guid id, Guid tenantId);
    Task<FAQ> CreateFAQAsync(FAQ faq);
    Task<FAQ?> UpdateFAQAsync(FAQ faq);
    Task<bool> DeleteFAQAsync(Guid id, Guid tenantId);

    // CMS Pages
    Task<List<CMSPage>> GetCMSPagesAsync(Guid tenantId, string? pageType);
    Task<CMSPage?> GetCMSPageByIdAsync(Guid id, Guid tenantId);
    Task<CMSPage?> GetCMSPageBySlugAsync(string slug, Guid tenantId);
    Task<CMSPage> CreateCMSPageAsync(CMSPage page);
    Task<CMSPage?> UpdateCMSPageAsync(CMSPage page);
    Task<bool> DeleteCMSPageAsync(Guid id, Guid tenantId);
}

public class CMSRepository : ICMSRepository
{
    private readonly string _connectionString;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<CMSRepository> _logger;

    public CMSRepository(
        string connectionString,
        ITenantContext tenantContext,
        ILogger<CMSRepository> logger)
    {
        _connectionString = connectionString;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    private NpgsqlConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    #region Blog Posts

    public async Task<BlogPost?> GetBlogPostByIdAsync(Guid id, Guid tenantId)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   title AS Title,
                   slug AS Slug,
                   content AS Content,
                   excerpt AS Excerpt,
                   author_id AS AuthorId,
                   author_name AS AuthorName,
                   category AS Category,
                   tags AS Tags,
                   featured_image AS FeaturedImage,
                   status AS Status,
                   views AS Views,
                   published_at AS PublishedAt,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM blog_posts
            WHERE id = @Id AND tenant_id = @TenantId";

        return await connection.QueryFirstOrDefaultAsync<BlogPost>(sql, new { Id = id, TenantId = tenantId });
    }

    public async Task<List<BlogPost>> GetBlogPostsAsync(
        Guid tenantId,
        int page,
        int limit,
        string? search,
        string? status)
    {
        using var connection = CreateConnection();
        
        var whereClauses = new List<string> { "tenant_id = @TenantId" };
        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);

        if (!string.IsNullOrEmpty(search))
        {
            whereClauses.Add("(title ILIKE @Search OR content ILIKE @Search OR author_name ILIKE @Search)");
            parameters.Add("Search", $"%{search}%");
        }

        if (!string.IsNullOrEmpty(status))
        {
            whereClauses.Add("status = @Status");
            parameters.Add("Status", status);
        }

        var whereClause = string.Join(" AND ", whereClauses);
        var offset = (page - 1) * limit;

        var sql = $@"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   title AS Title,
                   slug AS Slug,
                   content AS Content,
                   excerpt AS Excerpt,
                   author_id AS AuthorId,
                   author_name AS AuthorName,
                   category AS Category,
                   tags AS Tags,
                   featured_image AS FeaturedImage,
                   status AS Status,
                   views AS Views,
                   published_at AS PublishedAt,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM blog_posts
            WHERE {whereClause}
            ORDER BY created_at DESC
            LIMIT @Limit OFFSET @Offset";

        parameters.Add("Limit", limit);
        parameters.Add("Offset", offset);

        var posts = await connection.QueryAsync<BlogPost>(sql, parameters);
        return posts.ToList();
    }

    public async Task<int> GetBlogPostCountAsync(Guid tenantId, string? search, string? status)
    {
        using var connection = CreateConnection();
        
        var whereClauses = new List<string> { "tenant_id = @TenantId" };
        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);

        if (!string.IsNullOrEmpty(search))
        {
            whereClauses.Add("(title ILIKE @Search OR content ILIKE @Search OR author_name ILIKE @Search)");
            parameters.Add("Search", $"%{search}%");
        }

        if (!string.IsNullOrEmpty(status))
        {
            whereClauses.Add("status = @Status");
            parameters.Add("Status", status);
        }

        var whereClause = string.Join(" AND ", whereClauses);
        var sql = $"SELECT COUNT(*) FROM blog_posts WHERE {whereClause}";

        return await connection.ExecuteScalarAsync<int>(sql, parameters);
    }

    public async Task<BlogPost> CreateBlogPostAsync(BlogPost post)
    {
        using var connection = CreateConnection();
        
        post.Id = Guid.NewGuid();
        post.CreatedAt = DateTime.UtcNow;
        post.UpdatedAt = DateTime.UtcNow;

        if (post.Status == "published" && !post.PublishedAt.HasValue)
        {
            post.PublishedAt = DateTime.UtcNow;
        }

        const string sql = @"
            INSERT INTO blog_posts (
                id, tenant_id, title, slug, content, excerpt, author_id, author_name,
                category, tags, featured_image, status, views, published_at,
                created_at, updated_at
            )
            VALUES (
                @Id, @TenantId, @Title, @Slug, @Content, @Excerpt, @AuthorId, @AuthorName,
                @Category, @Tags, @FeaturedImage, @Status, @Views, @PublishedAt,
                @CreatedAt, @UpdatedAt
            )";

        await connection.ExecuteAsync(sql, post);

        _logger.LogInformation("Blog post created: {Title}", post.Title);
        return post;
    }

    public async Task<BlogPost?> UpdateBlogPostAsync(BlogPost post)
    {
        using var connection = CreateConnection();
        
        post.UpdatedAt = DateTime.UtcNow;

        if (post.Status == "published" && !post.PublishedAt.HasValue)
        {
            post.PublishedAt = DateTime.UtcNow;
        }

        const string sql = @"
            UPDATE blog_posts SET
                title = @Title,
                slug = @Slug,
                content = @Content,
                excerpt = @Excerpt,
                category = @Category,
                tags = @Tags,
                featured_image = @FeaturedImage,
                status = @Status,
                published_at = @PublishedAt,
                updated_at = @UpdatedAt
            WHERE id = @Id AND tenant_id = @TenantId";

        var rowsAffected = await connection.ExecuteAsync(sql, post);
        return rowsAffected > 0 ? post : null;
    }

    public async Task<bool> DeleteBlogPostAsync(Guid id, Guid tenantId)
    {
        using var connection = CreateConnection();
        
        const string sql = "DELETE FROM blog_posts WHERE id = @Id AND tenant_id = @TenantId";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, TenantId = tenantId });

        return rowsAffected > 0;
    }

    #endregion

    #region FAQs

    public async Task<List<FAQ>> GetFAQsAsync(Guid tenantId, string? category)
    {
        using var connection = CreateConnection();
        
        var whereClauses = new List<string> { "tenant_id = @TenantId" };
        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);

        if (!string.IsNullOrEmpty(category))
        {
            whereClauses.Add("category = @Category");
            parameters.Add("Category", category);
        }

        var whereClause = string.Join(" AND ", whereClauses);

        var sql = $@"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   question AS Question,
                   answer AS Answer,
                   category AS Category,
                   display_order AS DisplayOrder,
                   is_active AS IsActive,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM faqs
            WHERE {whereClause}
            ORDER BY display_order ASC, created_at ASC";

        var faqs = await connection.QueryAsync<FAQ>(sql, parameters);
        return faqs.ToList();
    }

    public async Task<FAQ?> GetFAQByIdAsync(Guid id, Guid tenantId)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   question AS Question,
                   answer AS Answer,
                   category AS Category,
                   display_order AS DisplayOrder,
                   is_active AS IsActive,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM faqs
            WHERE id = @Id AND tenant_id = @TenantId";

        return await connection.QueryFirstOrDefaultAsync<FAQ>(sql, new { Id = id, TenantId = tenantId });
    }

    public async Task<FAQ> CreateFAQAsync(FAQ faq)
    {
        using var connection = CreateConnection();
        
        faq.Id = Guid.NewGuid();
        faq.CreatedAt = DateTime.UtcNow;
        faq.UpdatedAt = DateTime.UtcNow;

        const string sql = @"
            INSERT INTO faqs (
                id, tenant_id, question, answer, category, display_order, is_active,
                created_at, updated_at
            )
            VALUES (
                @Id, @TenantId, @Question, @Answer, @Category, @DisplayOrder, @IsActive,
                @CreatedAt, @UpdatedAt
            )";

        await connection.ExecuteAsync(sql, faq);

        _logger.LogInformation("FAQ created: {Question}", faq.Question);
        return faq;
    }

    public async Task<FAQ?> UpdateFAQAsync(FAQ faq)
    {
        using var connection = CreateConnection();
        
        faq.UpdatedAt = DateTime.UtcNow;

        const string sql = @"
            UPDATE faqs SET
                question = @Question,
                answer = @Answer,
                category = @Category,
                display_order = @DisplayOrder,
                is_active = @IsActive,
                updated_at = @UpdatedAt
            WHERE id = @Id AND tenant_id = @TenantId";

        var rowsAffected = await connection.ExecuteAsync(sql, faq);
        return rowsAffected > 0 ? faq : null;
    }

    public async Task<bool> DeleteFAQAsync(Guid id, Guid tenantId)
    {
        using var connection = CreateConnection();
        
        const string sql = "DELETE FROM faqs WHERE id = @Id AND tenant_id = @TenantId";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, TenantId = tenantId });

        return rowsAffected > 0;
    }

    #endregion

    #region CMS Pages

    public async Task<List<CMSPage>> GetCMSPagesAsync(Guid tenantId, string? pageType)
    {
        using var connection = CreateConnection();
        
        var whereClauses = new List<string> { "tenant_id = @TenantId" };
        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);

        if (!string.IsNullOrEmpty(pageType))
        {
            whereClauses.Add("page_type = @PageType");
            parameters.Add("PageType", pageType);
        }

        var whereClause = string.Join(" AND ", whereClauses);

        var sql = $@"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   title AS Title,
                   slug AS Slug,
                   content AS Content,
                   page_type AS PageType,
                   status AS Status,
                   meta_title AS MetaTitle,
                   meta_description AS MetaDescription,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM cms_pages
            WHERE {whereClause}
            ORDER BY created_at DESC";

        var pages = await connection.QueryAsync<CMSPage>(sql, parameters);
        return pages.ToList();
    }

    public async Task<CMSPage?> GetCMSPageByIdAsync(Guid id, Guid tenantId)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   title AS Title,
                   slug AS Slug,
                   content AS Content,
                   page_type AS PageType,
                   status AS Status,
                   meta_title AS MetaTitle,
                   meta_description AS MetaDescription,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM cms_pages
            WHERE id = @Id AND tenant_id = @TenantId";

        return await connection.QueryFirstOrDefaultAsync<CMSPage>(sql, new { Id = id, TenantId = tenantId });
    }

    public async Task<CMSPage?> GetCMSPageBySlugAsync(string slug, Guid tenantId)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   title AS Title,
                   slug AS Slug,
                   content AS Content,
                   page_type AS PageType,
                   status AS Status,
                   meta_title AS MetaTitle,
                   meta_description AS MetaDescription,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM cms_pages
            WHERE slug = @Slug AND tenant_id = @TenantId AND status = 'published'";

        return await connection.QueryFirstOrDefaultAsync<CMSPage>(sql, new { Slug = slug, TenantId = tenantId });
    }

    public async Task<CMSPage> CreateCMSPageAsync(CMSPage page)
    {
        using var connection = CreateConnection();
        
        page.Id = Guid.NewGuid();
        page.CreatedAt = DateTime.UtcNow;
        page.UpdatedAt = DateTime.UtcNow;

        const string sql = @"
            INSERT INTO cms_pages (
                id, tenant_id, title, slug, content, page_type, status,
                meta_title, meta_description, created_at, updated_at
            )
            VALUES (
                @Id, @TenantId, @Title, @Slug, @Content, @PageType, @Status,
                @MetaTitle, @MetaDescription, @CreatedAt, @UpdatedAt
            )";

        await connection.ExecuteAsync(sql, page);

        _logger.LogInformation("CMS page created: {Title}", page.Title);
        return page;
    }

    public async Task<CMSPage?> UpdateCMSPageAsync(CMSPage page)
    {
        using var connection = CreateConnection();
        
        page.UpdatedAt = DateTime.UtcNow;

        const string sql = @"
            UPDATE cms_pages SET
                title = @Title,
                slug = @Slug,
                content = @Content,
                page_type = @PageType,
                status = @Status,
                meta_title = @MetaTitle,
                meta_description = @MetaDescription,
                updated_at = @UpdatedAt
            WHERE id = @Id AND tenant_id = @TenantId";

        var rowsAffected = await connection.ExecuteAsync(sql, page);
        return rowsAffected > 0 ? page : null;
    }

    public async Task<bool> DeleteCMSPageAsync(Guid id, Guid tenantId)
    {
        using var connection = CreateConnection();
        
        // Don't allow deletion of system pages
        const string checkSql = "SELECT page_type FROM cms_pages WHERE id = @Id AND tenant_id = @TenantId";
        var pageType = await connection.ExecuteScalarAsync<string>(checkSql, new { Id = id, TenantId = tenantId });

        if (pageType is "terms" or "privacy" or "about" or "contact")
        {
            _logger.LogWarning("Attempted to delete system page: {PageType}", pageType);
            return false;
        }

        const string sql = "DELETE FROM cms_pages WHERE id = @Id AND tenant_id = @TenantId";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, TenantId = tenantId });

        return rowsAffected > 0;
    }

    #endregion
}

