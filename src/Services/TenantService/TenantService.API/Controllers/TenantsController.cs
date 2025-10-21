using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;
using TenantService.Application.Commands;
using TenantService.Application.Queries;
using TenantService.Contracts.DTOs;
using SharedKernel.Auditing;
using SharedKernel.Models;
using Identity.Shared;

namespace TenantService.API.Controllers;

/// <summary>
/// Tenant management endpoints (SuperAdmin only, except for public lookup endpoints)
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize(Roles = "SuperAdmin")]
public class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<TenantsController> _logger;

    public TenantsController(
        IMediator mediator,
        IAuditService auditService,
        ICurrentUserService currentUser,
        ILogger<TenantsController> logger)
    {
        _mediator = mediator;
        _auditService = auditService;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Create a new tenant (SuperAdmin only)
    /// </summary>
    [HttpPost]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TenantDto>> Create([FromBody] CreateTenantRequest request)
    {
        _logger.LogInformation("SuperAdmin {UserId} creating tenant {TenantName} with subdomain {Subdomain}", 
            _currentUser.UserId, request.Name, request.Subdomain);

        var command = new CreateTenantCommand(request.Name, request.Subdomain, request.ContactEmail, request.ContactPhone);
        var result = await _mediator.Send(command);
        
        // Audit log
        await _auditService.LogAsync(new AuditEntry
        {
            TenantId = result.Id,
            UserId = _currentUser.UserId ?? "unknown",
            Action = "Create",
            EntityType = "Tenant",
            EntityId = result.Id,
            NewValues = System.Text.Json.JsonSerializer.Serialize(result),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
        });
        
        _logger.LogInformation("Tenant {TenantId} created successfully by SuperAdmin {UserId}", 
            result.Id, _currentUser.UserId);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Get tenant by ID (SuperAdmin only)
    /// </summary>
    [HttpGet("{id}")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TenantDto>> GetById(
        [FromRoute]
        [MaxLength(50)]
        string id)
    {
        _logger.LogInformation("SuperAdmin {UserId} getting tenant {TenantId}", 
            _currentUser.UserId, id);

        var query = new GetTenantByIdQuery(id);
        var result = await _mediator.Send(query);
        
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Get tenant by subdomain (public endpoint for tenant resolution)
    /// </summary>
    [HttpGet("subdomain/{subdomain}")]
    [AllowAnonymous]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(ApiResponse<TenantDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<TenantDto>>> GetBySubdomain(
        [FromRoute]
        [RegularExpression(@"^[a-zA-Z0-9-]{1,63}$", ErrorMessage = "Subdomain must contain only letters, numbers, and hyphens (max 63 characters)")]
        [MaxLength(63)]
        string subdomain)
    {
        _logger.LogInformation("Getting tenant by subdomain: {Subdomain}", subdomain);

        var query = new GetTenantBySubdomainQuery(subdomain);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(ApiResponse<TenantDto>.ErrorResponse("Tenant not found", new Dictionary<string, string[]>
            {
                { "subdomain", new[] { $"No tenant found with subdomain '{subdomain}'" } }
            }));
        }

        return Ok(ApiResponse<TenantDto>.SuccessResponse(result, "Tenant retrieved successfully"));
    }

    /// <summary>
    /// Get all active tenants (public endpoint for tenant selection)
    /// </summary>
    [HttpGet("public/active")]
    [AllowAnonymous]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TenantDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<TenantDto>>>> GetActiveTenants()
    {
        _logger.LogInformation("Getting all active tenants");

        var query = new GetActiveTenantsQuery();
        var result = await _mediator.Send(query);
        
        return Ok(ApiResponse<IEnumerable<TenantDto>>.SuccessResponse(result, "Active tenants retrieved successfully"));
    }
}


