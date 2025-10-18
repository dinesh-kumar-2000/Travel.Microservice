using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TenantService.Application.Commands;
using TenantService.Application.Queries;
using TenantService.Contracts.DTOs;
using SharedKernel.Auditing;
using SharedKernel.Models;

namespace TenantService.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAuditService _auditService;
    private readonly ILogger<TenantsController> _logger;

    public TenantsController(
        IMediator mediator,
        IAuditService auditService,
        ILogger<TenantsController> logger)
    {
        _mediator = mediator;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new tenant
    /// </summary>
    [HttpPost]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TenantDto>> Create([FromBody] CreateTenantRequest request)
    {
        _logger.LogInformation("Creating tenant {TenantName} with subdomain {Subdomain}", 
            request.Name, request.Subdomain);

        var command = new CreateTenantCommand(request.Name, request.Subdomain, request.ContactEmail, request.ContactPhone);
        var result = await _mediator.Send(command);
        
        // Audit log
        await _auditService.LogAsync(new AuditEntry
        {
            TenantId = result.Id,
            UserId = "system",
            Action = "Create",
            EntityType = "Tenant",
            EntityId = result.Id,
            NewValues = System.Text.Json.JsonSerializer.Serialize(result),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
        });
        
        _logger.LogInformation("Tenant {TenantId} created successfully", result.Id);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Get tenant by ID
    /// </summary>
    [HttpGet("{id}")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantDto>> GetById(string id)
    {
        _logger.LogInformation("Getting tenant {TenantId}", id);

        var query = new GetTenantByIdQuery(id);
        var result = await _mediator.Send(query);
        
        return result == null ? NotFound() : Ok(result);
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


