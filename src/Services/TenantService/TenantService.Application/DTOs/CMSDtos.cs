namespace TenantService.Application.DTOs;

#region Blog DTOs

public record BlogPostDto(
    Guid Id,
    Guid TenantId,
    string Title,
    string Slug,
    string Content,
    string? Excerpt,
    Guid AuthorId,
    string AuthorName,
    string? Category,
    List<string>? Tags,
    string? FeaturedImage,
    string Status,
    int Views,
    DateTime? PublishedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateBlogPostRequest(
    string Title,
    string Slug,
    string Content,
    string? Excerpt,
    string? Category,
    List<string>? Tags,
    string? FeaturedImage,
    string Status
);

public record UpdateBlogPostRequest(
    string Title,
    string Slug,
    string Content,
    string? Excerpt,
    string? Category,
    List<string>? Tags,
    string? FeaturedImage,
    string Status
);

#endregion

#region FAQ DTOs

public record FAQDto(
    Guid Id,
    Guid TenantId,
    string Question,
    string Answer,
    string Category,
    int DisplayOrder,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateFAQRequest(
    string Question,
    string Answer,
    string Category,
    int DisplayOrder,
    bool IsActive
);

public record UpdateFAQRequest(
    string Question,
    string Answer,
    string Category,
    int DisplayOrder,
    bool IsActive
);

#endregion

#region CMS Page DTOs

public record CMSPageDto(
    Guid Id,
    Guid TenantId,
    string Title,
    string Slug,
    string Content,
    string PageType,
    string Status,
    string? MetaTitle,
    string? MetaDescription,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateCMSPageRequest(
    string Title,
    string Slug,
    string Content,
    string PageType,
    string Status,
    string? MetaTitle,
    string? MetaDescription
);

public record UpdateCMSPageRequest(
    string Title,
    string Slug,
    string Content,
    string PageType,
    string Status,
    string? MetaTitle,
    string? MetaDescription
);

#endregion

public record PagedResult<T>(
    List<T> Data,
    int CurrentPage,
    int TotalPages,
    int TotalCount
);

