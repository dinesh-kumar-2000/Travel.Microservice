using CmsService.Domain.Entities;
using MediatR;
using SharedKernel.Models;

namespace CmsService.Application.Commands.Content;

public class CreateContentCommand : IRequest<CreateContentCommand.ContentDto>
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public Guid? TemplateId { get; set; }

    public class ContentDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
        public Guid? TemplateId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

public class CreateContentHandler : IRequestHandler<CreateContentCommand, CreateContentCommand.ContentDto>
{
    private readonly IContentRepository _contentRepository;

    public CreateContentHandler(IContentRepository contentRepository)
    {
        _contentRepository = contentRepository;
    }

    public async Task<CreateContentCommand.ContentDto> Handle(CreateContentCommand request, CancellationToken cancellationToken)
    {
        var content = new CmsContent
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Content = request.Content,
            Slug = request.Slug ?? GenerateSlug(request.Title),
            MetaDescription = request.MetaDescription,
            MetaKeywords = request.MetaKeywords,
            IsPublished = request.IsPublished,
            PublishedAt = request.IsPublished ? (request.PublishedAt ?? DateTime.UtcNow) : null,
            TemplateId = request.TemplateId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _contentRepository.AddAsync(content);

        return new CreateContentCommand.ContentDto
        {
            Id = content.Id,
            Title = content.Title,
            Content = content.Content,
            Slug = content.Slug,
            MetaDescription = content.MetaDescription,
            MetaKeywords = content.MetaKeywords,
            IsPublished = content.IsPublished,
            PublishedAt = content.PublishedAt,
            TemplateId = content.TemplateId,
            CreatedAt = content.CreatedAt,
            UpdatedAt = content.UpdatedAt
        };
    }

    private static string GenerateSlug(string title)
    {
        return title.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-")
            .Replace(".", "")
            .Replace(",", "")
            .Replace("!", "")
            .Replace("?", "")
            .Replace(":", "")
            .Replace(";", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("[", "")
            .Replace("]", "")
            .Replace("{", "")
            .Replace("}", "")
            .Replace("\"", "")
            .Replace("'", "")
            .Replace("&", "and")
            .Replace("@", "at")
            .Replace("#", "hash")
            .Replace("%", "percent")
            .Replace("+", "plus")
            .Replace("=", "equals")
            .Replace("$", "dollar")
            .Replace("^", "caret")
            .Replace("*", "star")
            .Replace("~", "tilde")
            .Replace("`", "")
            .Replace("|", "")
            .Replace("\\", "")
            .Replace("/", "")
            .Replace("<", "")
            .Replace(">", "");
    }
}
