using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IdentityService.Application.Commands;
using IdentityService.Application.DTOs;
using Identity.Shared;
using SharedKernel.Auditing;

namespace IdentityService.API.Controllers;

/// <summary>
/// SuperAdmin-only endpoints for system administration
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize(Roles = "SuperAdmin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IMediator mediator,
        ICurrentUserService currentUser,
        IAuditService auditService,
        ILogger<AdminController> logger)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new tenant (SuperAdmin only - must be accessed from main domain)
    /// </summary>
    [HttpPost("tenants")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> CreateTenant([FromBody] CreateTenantRequest request)
    {
        // This endpoint will call the TenantService
        _logger.LogInformation("SuperAdmin {UserId} is creating a new tenant", _currentUser.UserId);

        // Here you would typically make an HTTP call to TenantService
        // or use a message bus to send a command
        
        await _auditService.LogAsync(new AuditEntry
        {
            TenantId = "system",
            UserId = _currentUser.UserId!,
            Action = "CreateTenant",
            EntityType = "Tenant",
            EntityId = request.Name,
            NewValues = System.Text.Json.JsonSerializer.Serialize(request),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
        });

        return StatusCode(StatusCodes.Status201Created, new { message = "Tenant creation initiated" });
    }

    /// <summary>
    /// Create TenantAdmin user for a specific tenant
    /// </summary>
    [HttpPost("tenants/{tenantId}/admin")]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<RegisterUserResponse>> CreateTenantAdmin(
        string tenantId, 
        [FromBody] CreateTenantAdminRequest request)
    {
        _logger.LogInformation("SuperAdmin {UserId} is creating TenantAdmin for tenant {TenantId}", 
            _currentUser.UserId, tenantId);

        var command = new RegisterUserCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.PhoneNumber ?? string.Empty,
            tenantId,
            "TenantAdmin" // Specify role
        );

        var result = await _mediator.Send(command);

        await _auditService.LogAsync(new AuditEntry
        {
            TenantId = "system",
            UserId = _currentUser.UserId!,
            Action = "CreateTenantAdmin",
            EntityType = "User",
            EntityId = result.User.Id,
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { request.Email, tenantId, Role = "TenantAdmin" }),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
        });

        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Get all users across all tenants (SuperAdmin only)
    /// </summary>
    [HttpGet("users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> GetAllUsers()
    {
        _logger.LogInformation("SuperAdmin {UserId} is viewing all users", _currentUser.UserId);
        
        // Implementation would query all users across all tenants
        return Ok(new { message = "List of all users across tenants" });
    }
}

public record CreateTenantRequest(
    string Name,
    string Subdomain,
    string ContactEmail,
    string ContactPhone
);

public record CreateTenantAdminRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber
);

