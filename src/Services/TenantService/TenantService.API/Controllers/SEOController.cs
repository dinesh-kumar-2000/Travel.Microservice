using TenantService.Application.Commands;
using TenantService.Application.Queries;
using TenantService.Contracts.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TenantService.API.Controllers;

[ApiController]
[Route("api/tenantadmin/seo")]
[Authorize(Roles = "TenantAdmin")]
public class SEOController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SEOController> _logger;

    public SEOController(IMediator mediator, ILogger<SEOController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get SEO settings for tenant
    /// </summary>
    [HttpGet("settings")]
    public async Task<ActionResult<SEOSettingsDto>> GetSettings()
    {
        var tenantId = GetTenantId();
        var query = new GetSEOSettingsQuery(tenantId);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            // Return default settings if none exist
            return Ok(new SEOSettingsDto(
                tenantId,
                null, null, null, null, null, // General
                null, null, null, null, null, // OpenGraph
                null, null, null, null, null, // Twitter
                "index, follow", null, null, null, null, // Technical
                true, null, null, // Sitemap
                true, null, true, true, // Schema
                DateTime.UtcNow,
                DateTime.UtcNow
            ));
        }

        return Ok(result);
    }

    /// <summary>
    /// Update SEO settings
    /// </summary>
    [HttpPut("settings")]
    public async Task<ActionResult<SEOSettingsDto>> UpdateSettings([FromBody] UpdateSEOSettingsRequest request)
    {
        var tenantId = GetTenantId();

        var command = new UpdateSEOSettingsCommand(
            tenantId,
            request.General,
            request.OpenGraph,
            request.Twitter,
            request.Technical,
            request.Sitemap,
            request.Schema
        );

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Generate sitemap for tenant
    /// </summary>
    [HttpPost("sitemap/generate")]
    public async Task<ActionResult<SitemapResponseDto>> GenerateSitemap()
    {
        var tenantId = GetTenantId();
        var command = new GenerateSitemapCommand(tenantId);
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    private Guid GetTenantId()
    {
        var tenantId = User.FindFirst("tenantId")?.Value;
        return string.IsNullOrEmpty(tenantId) ? Guid.Empty : Guid.Parse(tenantId);
    }
}

