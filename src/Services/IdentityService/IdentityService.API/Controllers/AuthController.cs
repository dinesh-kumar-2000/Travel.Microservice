using MediatR;
using Microsoft.AspNetCore.Mvc;
using IdentityService.Application.Commands;
using IdentityService.Application.Queries;
using IdentityService.Contracts.DTOs;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
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
            request.PhoneNumber ?? string.Empty,
            request.TenantId
        );

        var result = await _mediator.Send(command);
        
        // Audit log
        await _auditService.LogAsync(new AuditEntry
        {
            TenantId = request.TenantId,
            UserId = result.User.Id,
            Action = "Register",
            EntityType = "User",
            EntityId = result.User.Id,
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { result.User.Email, request.FirstName, request.LastName }),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
        });
        
        _logger.LogInformation("User {Email} registered successfully with ID {UserId}", 
            request.Email, result.User.Id);

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
        _logger.LogInformation("Login attempt for user {Email} from domain {Domain}", 
            request.Email, request.Domain ?? request.TenantId ?? "unknown");

        var command = new LoginCommand(
            request.Email,
            request.Password,
            request.Domain,
            request.TenantId
        );

        var result = await _mediator.Send(command);
        
        // Audit log
        await _auditService.LogAsync(new AuditEntry
        {
            TenantId = request.TenantId ?? result.User.Id,
            UserId = result.User.Id,
            Action = "Login",
            EntityType = "User",
            EntityId = result.User.Id,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
        });
        
        _logger.LogInformation("User {Email} logged in successfully with roles: {Roles}", 
            request.Email, string.Join(", ", result.User.Roles));

        return Ok(result);
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh-token")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        _logger.LogInformation("Refresh token request for user {UserId}", request.UserId);

        var command = new RefreshTokenCommand(
            request.RefreshToken,
            request.UserId
        );

        var result = await _mediator.Send(command);
        
        // Audit log
        await _auditService.LogAsync(new AuditEntry
        {
            TenantId = "system",
            UserId = request.UserId,
            Action = "RefreshToken",
            EntityType = "User",
            EntityId = request.UserId,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
        });
        
        _logger.LogInformation("Token refreshed successfully for user {UserId}", request.UserId);

        return Ok(result);
    }

    /// <summary>
    /// Revoke refresh token (logout)
    /// </summary>
    [HttpPost("revoke-token")]
    [Authorize]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(RevokeTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RevokeTokenResponse>> RevokeToken([FromBody] RevokeTokenRequest request)
    {
        _logger.LogInformation("Revoke token request for user {UserId}", request.UserId);

        var command = new RevokeTokenCommand(request.UserId);

        var result = await _mediator.Send(command);
        
        // Audit log
        await _auditService.LogAsync(new AuditEntry
        {
            TenantId = "system",
            UserId = request.UserId,
            Action = "RevokeToken",
            EntityType = "User",
            EntityId = request.UserId,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
        });
        
        _logger.LogInformation("Token revoked successfully for user {UserId}", request.UserId);

        return Ok(result);
    }

    /// <summary>
    /// Get user profile
    /// </summary>
    [HttpGet("profile/{userId}")]
    [Authorize]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(GetProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetProfileResponse>> GetProfile(string userId)
    {
        _logger.LogInformation("Get profile request for user {UserId}", userId);

        var query = new GetProfileQuery(userId);

        var result = await _mediator.Send(query);
        
        _logger.LogInformation("Profile retrieved successfully for user {UserId}", userId);

        return Ok(result);
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(UpdateProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdateProfileResponse>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        _logger.LogInformation("Update profile request for user {UserId}", request.UserId);

        var command = new UpdateProfileCommand(
            request.UserId,
            request.FirstName,
            request.LastName,
            request.PhoneNumber
        );

        var result = await _mediator.Send(command);
        
        // Audit log
        await _auditService.LogAsync(new AuditEntry
        {
            TenantId = "system",
            UserId = request.UserId,
            Action = "UpdateProfile",
            EntityType = "User",
            EntityId = request.UserId,
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { request.FirstName, request.LastName, request.PhoneNumber }),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
        });
        
        _logger.LogInformation("Profile updated successfully for user {UserId}", request.UserId);

        return Ok(result);
    }
}


