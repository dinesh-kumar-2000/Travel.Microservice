using MediatR;
using Microsoft.AspNetCore.Mvc;
using IdentityService.Application.Commands;
using IdentityService.Contracts.DTOs;
using Microsoft.AspNetCore.RateLimiting;
using SharedKernel.Auditing;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAuditService _auditService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IMediator mediator,
        IAuditService auditService,
        ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegisterUserResponse>> Register([FromBody] RegisterUserRequest request)
    {
        _logger.LogInformation("Registering user {Email} for tenant {TenantId}", 
            request.Email, request.TenantId);

        var command = new RegisterUserCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            request.TenantId
        );

        var result = await _mediator.Send(command);
        
        // Audit log
        await _auditService.LogAsync(new AuditEntry
        {
            TenantId = request.TenantId,
            UserId = result.UserId,
            Action = "Register",
            EntityType = "User",
            EntityId = result.UserId,
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { result.Email, request.FirstName, request.LastName }),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
        });
        
        _logger.LogInformation("User {Email} registered successfully with ID {UserId}", 
            request.Email, result.UserId);

        return Ok(result);
    }

    /// <summary>
    /// Login user and get JWT token
    /// </summary>
    [HttpPost("login")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login attempt for user {Email} in tenant {TenantId}", 
            request.Email, request.TenantId);

        var command = new LoginCommand(
            request.Email,
            request.Password,
            request.TenantId
        );

        var result = await _mediator.Send(command);
        
        // Audit log
        await _auditService.LogAsync(new AuditEntry
        {
            TenantId = request.TenantId,
            UserId = result.User.Id,
            Action = "Login",
            EntityType = "User",
            EntityId = result.User.Id,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
        });
        
        _logger.LogInformation("User {Email} logged in successfully", request.Email);

        return Ok(result);
    }
}


