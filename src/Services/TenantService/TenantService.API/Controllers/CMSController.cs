using TenantService.Application.Commands;
using TenantService.Application.Queries;
using TenantService.Contracts.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TenantService.API.Controllers;

[ApiController]
[Route("api/tenantadmin/cms")]
[Authorize(Roles = "TenantAdmin")]
public class CMSController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CMSController> _logger;

    public CMSController(IMediator mediator, ILogger<CMSController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    #region Blog Posts

    /// <summary>
    /// Get all blog posts
    /// </summary>
    [HttpGet("blog")]
    public async Task<ActionResult<PagedResult<BlogPostDto>>> GetBlogPosts(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null)
    {
        var tenantId = GetTenantId();
        var query = new GetBlogPostsQuery(tenantId, page, limit, search, status);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Get blog post by ID
    /// </summary>
    [HttpGet("blog/{id}")]
    public async Task<ActionResult<BlogPostDto>> GetBlogPost(Guid id)
    {
        var tenantId = GetTenantId();
        var query = new GetBlogPostByIdQuery(id, tenantId);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Create blog post
    /// </summary>
    [HttpPost("blog")]
    public async Task<ActionResult<BlogPostDto>> CreateBlogPost([FromBody] CreateBlogPostRequest request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var userName = GetUserName();

        var command = new CreateBlogPostCommand(
            tenantId,
            userId,
            userName,
            request.Title,
            request.Slug,
            request.Content,
            request.Excerpt,
            request.Category,
            request.Tags,
            request.FeaturedImage,
            request.Status
        );

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetBlogPost), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update blog post
    /// </summary>
    [HttpPut("blog/{id}")]
    public async Task<ActionResult<BlogPostDto>> UpdateBlogPost(Guid id, [FromBody] UpdateBlogPostRequest request)
    {
        var tenantId = GetTenantId();

        var command = new UpdateBlogPostCommand(
            id,
            tenantId,
            request.Title,
            request.Slug,
            request.Content,
            request.Excerpt,
            request.Category,
            request.Tags,
            request.FeaturedImage,
            request.Status
        );

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Delete blog post
    /// </summary>
    [HttpDelete("blog/{id}")]
    public async Task<ActionResult> DeleteBlogPost(Guid id)
    {
        var tenantId = GetTenantId();
        var command = new DeleteBlogPostCommand(id, tenantId);
        await _mediator.Send(command);

        return NoContent();
    }

    #endregion

    #region FAQs

    /// <summary>
    /// Get all FAQs
    /// </summary>
    [HttpGet("faq")]
    public async Task<ActionResult<List<FAQDto>>> GetFAQs([FromQuery] string? category = null)
    {
        var tenantId = GetTenantId();
        var query = new GetFAQsQuery(tenantId, category);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Create FAQ
    /// </summary>
    [HttpPost("faq")]
    public async Task<ActionResult<FAQDto>> CreateFAQ([FromBody] CreateFAQRequest request)
    {
        var tenantId = GetTenantId();

        var command = new CreateFAQCommand(
            tenantId,
            request.Question,
            request.Answer,
            request.Category,
            request.DisplayOrder,
            request.IsActive
        );

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetFAQs), new { }, result);
    }

    /// <summary>
    /// Update FAQ
    /// </summary>
    [HttpPut("faq/{id}")]
    public async Task<ActionResult<FAQDto>> UpdateFAQ(Guid id, [FromBody] UpdateFAQRequest request)
    {
        var tenantId = GetTenantId();

        var command = new UpdateFAQCommand(
            id,
            tenantId,
            request.Question,
            request.Answer,
            request.Category,
            request.DisplayOrder,
            request.IsActive
        );

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Delete FAQ
    /// </summary>
    [HttpDelete("faq/{id}")]
    public async Task<ActionResult> DeleteFAQ(Guid id)
    {
        var tenantId = GetTenantId();
        var command = new DeleteFAQCommand(id, tenantId);
        await _mediator.Send(command);

        return NoContent();
    }

    #endregion

    #region CMS Pages

    /// <summary>
    /// Get all CMS pages
    /// </summary>
    [HttpGet("pages")]
    public async Task<ActionResult<List<CMSPageDto>>> GetPages([FromQuery] string? pageType = null)
    {
        var tenantId = GetTenantId();
        var query = new GetCMSPagesQuery(tenantId, pageType);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Get CMS page by ID
    /// </summary>
    [HttpGet("pages/{id}")]
    public async Task<ActionResult<CMSPageDto>> GetPage(Guid id)
    {
        var tenantId = GetTenantId();
        var query = new GetCMSPageByIdQuery(id, tenantId);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Create CMS page
    /// </summary>
    [HttpPost("pages")]
    public async Task<ActionResult<CMSPageDto>> CreatePage([FromBody] CreateCMSPageRequest request)
    {
        var tenantId = GetTenantId();

        var command = new CreateCMSPageCommand(
            tenantId,
            request.Title,
            request.Slug,
            request.Content,
            request.PageType,
            request.Status,
            request.MetaTitle,
            request.MetaDescription
        );

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetPage), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update CMS page
    /// </summary>
    [HttpPut("pages/{id}")]
    public async Task<ActionResult<CMSPageDto>> UpdatePage(Guid id, [FromBody] UpdateCMSPageRequest request)
    {
        var tenantId = GetTenantId();

        var command = new UpdateCMSPageCommand(
            id,
            tenantId,
            request.Title,
            request.Slug,
            request.Content,
            request.PageType,
            request.Status,
            request.MetaTitle,
            request.MetaDescription
        );

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Delete CMS page
    /// </summary>
    [HttpDelete("pages/{id}")]
    public async Task<ActionResult> DeletePage(Guid id)
    {
        var tenantId = GetTenantId();
        var command = new DeleteCMSPageCommand(id, tenantId);
        await _mediator.Send(command);

        return NoContent();
    }

    #endregion

    private Guid GetTenantId()
    {
        var tenantId = User.FindFirst("tenantId")?.Value;
        return string.IsNullOrEmpty(tenantId) ? Guid.Empty : Guid.Parse(tenantId);
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
        return string.IsNullOrEmpty(userId) ? Guid.Empty : Guid.Parse(userId);
    }

    private string GetUserName()
    {
        return User.FindFirst("name")?.Value ?? User.FindFirst("email")?.Value ?? "Unknown";
    }
}

