using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TenantService.Application.Commands;
using TenantService.Application.Queries;
using TenantService.Contracts.DTOs;
using SharedKernel.Auditing;
using SharedKernel.Models;
using Identity.Shared;

namespace TenantService.API.Controllers;

/// <summary>
/// Landing page management endpoints for dynamic page creation and customization
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class LandingPagesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<LandingPagesController> _logger;

    public LandingPagesController(
        IMediator mediator,
        IAuditService auditService,
        ICurrentUserService currentUser,
        ILogger<LandingPagesController> logger)
    {
        _mediator = mediator;
        _auditService = auditService;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Create a new landing page (TenantAdmin, SuperAdmin)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "TenantAdmin,SuperAdmin")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(ApiResponse<LandingPageDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<LandingPageDto>>> Create([FromBody] CreateLandingPageRequest request)
    {
        _logger.LogInformation("User {UserId} creating landing page {PageName} for tenant {TenantId}", 
            _currentUser.UserId, request.PageName, request.TenantId);

        var command = new CreateLandingPageCommand(request, _currentUser.UserId);
        var result = await _mediator.Send(command);
        
        // Audit log
        await _auditService.LogAsync(new AuditEntry
        {
            TenantId = result.TenantId,
            UserId = _currentUser.UserId ?? "unknown",
            Action = "Create",
            EntityType = "LandingPage",
            EntityId = result.PageId,
            NewValues = System.Text.Json.JsonSerializer.Serialize(result),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
        });
        
        _logger.LogInformation("Landing page {PageId} created successfully by user {UserId}", 
            result.PageId, _currentUser.UserId);

        return CreatedAtAction(
            nameof(GetById), 
            new { pageId = result.PageId }, 
            ApiResponse<LandingPageDto>.SuccessResponse(result, "Landing page created successfully")
        );
    }

    /// <summary>
    /// Get landing page by ID (TenantAdmin, SuperAdmin)
    /// </summary>
    [HttpGet("{pageId}")]
    [Authorize(Roles = "TenantAdmin,SuperAdmin")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(ApiResponse<LandingPageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LandingPageDto>>> GetById(string pageId)
    {
        _logger.LogInformation("User {UserId} getting landing page {PageId}", _currentUser.UserId, pageId);

        var query = new GetLandingPageByIdQuery(pageId);
        var result = await _mediator.Send(query);
        
        if (result == null)
        {
            return NotFound(ApiResponse<LandingPageDto>.ErrorResponse(
                "Landing page not found", 
                new Dictionary<string, string[]> { { "pageId", new[] { $"No landing page found with ID '{pageId}'" } } }
            ));
        }

        return Ok(ApiResponse<LandingPageDto>.SuccessResponse(result, "Landing page retrieved successfully"));
    }

    /// <summary>
    /// Get landing page by slug (Public endpoint for rendering)
    /// </summary>
    [HttpGet("tenant/{tenantId}/slug/{slug}")]
    [AllowAnonymous]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(ApiResponse<LandingPageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LandingPageDto>>> GetBySlug(
        string tenantId, 
        string slug, 
        [FromQuery] string language = "en")
    {
        _logger.LogInformation("Getting landing page by slug: {Slug} for tenant {TenantId}, language {Language}", 
            slug, tenantId, language);

        var query = new GetLandingPageBySlugQuery(tenantId, slug, language);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(ApiResponse<LandingPageDto>.ErrorResponse(
                "Landing page not found",
                new Dictionary<string, string[]> 
                { 
                    { "slug", new[] { $"No landing page found with slug '{slug}' for tenant '{tenantId}'" } } 
                }
            ));
        }

        // Only return published pages for public access
        if (result.Status != "Published" && !User.IsInRole("TenantAdmin") && !User.IsInRole("SuperAdmin"))
        {
            return NotFound(ApiResponse<LandingPageDto>.ErrorResponse(
                "Landing page not available",
                new Dictionary<string, string[]> { { "status", new[] { "This page is not published yet" } } }
            ));
        }

        return Ok(ApiResponse<LandingPageDto>.SuccessResponse(result, "Landing page retrieved successfully"));
    }

    /// <summary>
    /// Get landing page by subdomain and slug (Public endpoint - most common use case)
    /// </summary>
    [HttpGet("subdomain/{subdomain}/slug/{**slug}")]
    [AllowAnonymous]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(ApiResponse<LandingPageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LandingPageDto>>> GetBySubdomainAndSlug(
        string subdomain,
        string slug = "/",
        [FromQuery] string language = "en")
    {
        _logger.LogInformation("Getting landing page by subdomain: {Subdomain}, slug: {Slug}, language: {Language}",
            subdomain, slug, language);

        var query = new GetLandingPageBySubdomainQuery(subdomain, slug, language);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(ApiResponse<LandingPageDto>.ErrorResponse(
                "Landing page not found",
                new Dictionary<string, string[]>
                {
                    { "subdomain", new[] { $"No landing page found with slug '{slug}' for subdomain '{subdomain}'" } }
                }
            ));
        }

        // Only return published pages for public access
        if (result.Status != "Published" && !User.IsInRole("TenantAdmin") && !User.IsInRole("SuperAdmin"))
        {
            return NotFound(ApiResponse<LandingPageDto>.ErrorResponse(
                "Landing page not available",
                new Dictionary<string, string[]> { { "status", new[] { "This page is not published yet" } } }
            ));
        }

        return Ok(ApiResponse<LandingPageDto>.SuccessResponse(result, "Landing page retrieved successfully"));
    }

    /// <summary>
    /// Get all landing pages for a tenant (TenantAdmin, SuperAdmin)
    /// </summary>
    [HttpGet("tenant/{tenantId}")]
    [Authorize(Roles = "TenantAdmin,SuperAdmin")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LandingPageSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<LandingPageSummaryDto>>>> GetByTenant(
        string tenantId,
        [FromQuery] bool publishedOnly = false)
    {
        _logger.LogInformation("User {UserId} getting landing pages for tenant {TenantId}", 
            _currentUser.UserId, tenantId);

        var query = new GetLandingPagesByTenantQuery(tenantId, publishedOnly);
        var result = await _mediator.Send(query);
        
        return Ok(ApiResponse<IEnumerable<LandingPageSummaryDto>>.SuccessResponse(
            result, 
            "Landing pages retrieved successfully"
        ));
    }

    /// <summary>
    /// Search landing pages for a tenant (TenantAdmin, SuperAdmin)
    /// </summary>
    [HttpGet("tenant/{tenantId}/search")]
    [Authorize(Roles = "TenantAdmin,SuperAdmin")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(ApiResponse<SearchLandingPagesResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SearchLandingPagesResult>>> Search(
        string tenantId,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        _logger.LogInformation("User {UserId} searching landing pages for tenant {TenantId} with term '{SearchTerm}'", 
            _currentUser.UserId, tenantId, searchTerm);

        var query = new SearchLandingPagesQuery(tenantId, searchTerm, status, page, pageSize);
        var result = await _mediator.Send(query);
        
        return Ok(ApiResponse<SearchLandingPagesResult>.SuccessResponse(
            result, 
            $"Found {result.TotalCount} landing pages"
        ));
    }

    /// <summary>
    /// Update a landing page (TenantAdmin, SuperAdmin)
    /// </summary>
    [HttpPut("{pageId}")]
    [Authorize(Roles = "TenantAdmin,SuperAdmin")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(ApiResponse<LandingPageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<LandingPageDto>>> Update(
        string pageId, 
        [FromBody] UpdateLandingPageRequest request)
    {
        _logger.LogInformation("User {UserId} updating landing page {PageId}", _currentUser.UserId, pageId);

        var command = new UpdateLandingPageCommand(pageId, request, _currentUser.UserId);
        var result = await _mediator.Send(command);
        
        // Audit log
        await _auditService.LogAsync(new AuditEntry
        {
            TenantId = result.TenantId,
            UserId = _currentUser.UserId ?? "unknown",
            Action = "Update",
            EntityType = "LandingPage",
            EntityId = result.PageId,
            NewValues = System.Text.Json.JsonSerializer.Serialize(result),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
        });
        
        _logger.LogInformation("Landing page {PageId} updated successfully by user {UserId}", 
            result.PageId, _currentUser.UserId);

        return Ok(ApiResponse<LandingPageDto>.SuccessResponse(result, "Landing page updated successfully"));
    }

    /// <summary>
    /// Delete a landing page (TenantAdmin, SuperAdmin)
    /// </summary>
    [HttpDelete("{pageId}")]
    [Authorize(Roles = "TenantAdmin,SuperAdmin")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(string pageId)
    {
        _logger.LogInformation("User {UserId} deleting landing page {PageId}", _currentUser.UserId, pageId);

        var command = new DeleteLandingPageCommand(pageId);
        var result = await _mediator.Send(command);
        
        // Audit log
        await _auditService.LogAsync(new AuditEntry
        {
            UserId = _currentUser.UserId ?? "unknown",
            Action = "Delete",
            EntityType = "LandingPage",
            EntityId = pageId,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
        });
        
        _logger.LogInformation("Landing page {PageId} deleted successfully by user {UserId}", 
            pageId, _currentUser.UserId);

        return Ok(ApiResponse<bool>.SuccessResponse(result, "Landing page deleted successfully"));
    }

    /// <summary>
    /// Publish a landing page (TenantAdmin, SuperAdmin)
    /// </summary>
    [HttpPost("{pageId}/publish")]
    [Authorize(Roles = "TenantAdmin,SuperAdmin")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(ApiResponse<LandingPageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LandingPageDto>>> Publish(string pageId)
    {
        _logger.LogInformation("User {UserId} publishing landing page {PageId}", _currentUser.UserId, pageId);

        var command = new PublishLandingPageCommand(pageId, _currentUser.UserId);
        var result = await _mediator.Send(command);
        
        // Audit log
        await _auditService.LogAsync(new AuditEntry
        {
            TenantId = result.TenantId,
            UserId = _currentUser.UserId ?? "unknown",
            Action = "Publish",
            EntityType = "LandingPage",
            EntityId = result.PageId,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
        });
        
        _logger.LogInformation("Landing page {PageId} published successfully by user {UserId}", 
            result.PageId, _currentUser.UserId);

        return Ok(ApiResponse<LandingPageDto>.SuccessResponse(result, "Landing page published successfully"));
    }

    /// <summary>
    /// Unpublish a landing page (TenantAdmin, SuperAdmin)
    /// </summary>
    [HttpPost("{pageId}/unpublish")]
    [Authorize(Roles = "TenantAdmin,SuperAdmin")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(ApiResponse<LandingPageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LandingPageDto>>> Unpublish(string pageId)
    {
        _logger.LogInformation("User {UserId} unpublishing landing page {PageId}", _currentUser.UserId, pageId);

        var command = new UnpublishLandingPageCommand(pageId, _currentUser.UserId);
        var result = await _mediator.Send(command);
        
        // Audit log
        await _auditService.LogAsync(new AuditEntry
        {
            TenantId = result.TenantId,
            UserId = _currentUser.UserId ?? "unknown",
            Action = "Unpublish",
            EntityType = "LandingPage",
            EntityId = result.PageId,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
        });

        return Ok(ApiResponse<LandingPageDto>.SuccessResponse(result, "Landing page unpublished successfully"));
    }

    /// <summary>
    /// Archive a landing page (TenantAdmin, SuperAdmin)
    /// </summary>
    [HttpPost("{pageId}/archive")]
    [Authorize(Roles = "TenantAdmin,SuperAdmin")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(ApiResponse<LandingPageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LandingPageDto>>> Archive(string pageId)
    {
        _logger.LogInformation("User {UserId} archiving landing page {PageId}", _currentUser.UserId, pageId);

        var command = new ArchiveLandingPageCommand(pageId, _currentUser.UserId);
        var result = await _mediator.Send(command);
        
        // Audit log
        await _auditService.LogAsync(new AuditEntry
        {
            TenantId = result.TenantId,
            UserId = _currentUser.UserId ?? "unknown",
            Action = "Archive",
            EntityType = "LandingPage",
            EntityId = result.PageId,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
        });

        return Ok(ApiResponse<LandingPageDto>.SuccessResponse(result, "Landing page archived successfully"));
    }
}

