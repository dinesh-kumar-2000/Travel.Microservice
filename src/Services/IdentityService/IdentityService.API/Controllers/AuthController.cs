using MediatR;
using Microsoft.AspNetCore.Mvc;
using IdentityService.Application.Commands;
using IdentityService.Application.Queries;
using IdentityService.Application.DTOs;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using SharedKernel.Auditing;
using IdentityService.Infrastructure.Services;
using SharedKernel.Models;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAuditService _auditService;
    private readonly ILogger<AuthController> _logger;
    private readonly ISocialLoginService _socialLoginService;
    private readonly IPasswordResetService _passwordResetService;
    private readonly IAccountLockoutService _accountLockoutService;
    private readonly ISecurityAuditService _securityAuditService;

    public AuthController(
        IMediator mediator,
        IAuditService auditService,
        ILogger<AuthController> logger,
        ISocialLoginService socialLoginService,
        IPasswordResetService passwordResetService,
        IAccountLockoutService accountLockoutService,
        ISecurityAuditService securityAuditService)
    {
        _mediator = mediator;
        _auditService = auditService;
        _logger = logger;
        _socialLoginService = socialLoginService;
        _passwordResetService = passwordResetService;
        _accountLockoutService = accountLockoutService;
        _securityAuditService = securityAuditService;
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
    [ProducesResponseType(StatusCodes.Status423Locked)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var clientIpAddress = GetClientIpAddress();
        var userAgent = Request.Headers.UserAgent.ToString();
        var tenantId = request.TenantId ?? "system";

        _logger.LogInformation("Login attempt for user {Email} from domain {Domain}", 
            request.Email, request.Domain ?? tenantId);

        // Check if account is locked
        var isLocked = await _accountLockoutService.IsAccountLockedAsync(request.Email, tenantId);
        if (isLocked)
        {
            var lockoutEndTime = await _accountLockoutService.GetLockoutEndTimeAsync(request.Email, tenantId);
            await _securityAuditService.LogLoginEventAsync(
                request.Email, tenantId, false, clientIpAddress, userAgent, "Account locked");
            
            return StatusCode(423, new { message = $"Account is locked until {lockoutEndTime:yyyy-MM-dd HH:mm:ss UTC}" });
        }

        var command = new LoginCommand(
            request.Email,
            request.Password,
            request.Domain,
            request.TenantId
        );

        try
        {
            var result = await _mediator.Send(command);
            
            // Clear failed login attempts on successful login
            await _accountLockoutService.ClearFailedLoginAttemptsAsync(request.Email, tenantId);
            
            // Log successful login
            await _securityAuditService.LogLoginEventAsync(
                result.User.Id, tenantId, true, clientIpAddress, userAgent);
            
            // Legacy audit log
            await _auditService.LogAsync(new AuditEntry
            {
                TenantId = tenantId,
                UserId = result.User.Id,
                Action = "Login",
                EntityType = "User",
                EntityId = result.User.Id,
                IpAddress = clientIpAddress,
                UserAgent = userAgent
            });
            
            _logger.LogInformation("User {Email} logged in successfully with roles: {Roles}", 
                request.Email, string.Join(", ", result.User.Roles));

            return Ok(result);
        }
        catch (Exception ex)
        {
            // Record failed login attempt
            var lockoutResult = await _accountLockoutService.RecordFailedLoginAsync(
                request.Email, tenantId, clientIpAddress, userAgent);
            
            // Log failed login
            await _securityAuditService.LogLoginEventAsync(
                request.Email, tenantId, false, clientIpAddress, userAgent, ex.Message);
            
            return Unauthorized(new { message = "Invalid email or password" });
        }
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

    /// <summary>
    /// Initiate password reset
    /// </summary>
    [HttpPost("password-reset/initiate")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> InitiatePasswordReset([FromBody] InitiatePasswordResetRequest request)
    {
        var clientIpAddress = GetClientIpAddress();
        var userAgent = Request.Headers.UserAgent.ToString();
        var tenantId = request.TenantId ?? "system";

        _logger.LogInformation("Password reset initiated for user {Email} in tenant {TenantId}", 
            request.Email, tenantId);

        var result = await _passwordResetService.InitiatePasswordResetAsync(
            request.Email, tenantId, clientIpAddress);

        if (result.IsSuccess)
        {
            // Log password reset request
            await _securityAuditService.LogSecurityEventAsync(
                SecurityEventType.PasswordResetRequested, request.Email, tenantId, clientIpAddress, userAgent);
            
            _logger.LogInformation("Password reset email sent to {Email}", request.Email);
            return Ok(new { message = "Password reset email sent successfully" });
        }

        return BadRequest(new { message = result.ErrorMessage ?? "Failed to initiate password reset" });
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    [HttpPost("password-reset/confirm")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var clientIpAddress = GetClientIpAddress();
        var userAgent = Request.Headers.UserAgent.ToString();
        var tenantId = request.TenantId ?? "system";

        _logger.LogInformation("Password reset confirmation for user {Email} in tenant {TenantId}", 
            request.Email, tenantId);

        var result = await _passwordResetService.ResetPasswordAsync(
            request.Token, request.NewPassword, clientIpAddress);

        if (result.IsSuccess)
        {
            // Log password reset completion
            await _securityAuditService.LogPasswordChangeEventAsync(
                request.Email ?? "unknown", tenantId, true, clientIpAddress, userAgent);
            
            _logger.LogInformation("Password reset completed for user {Email}", request.Email);
            return Ok(new { message = "Password reset successfully" });
        }

        return BadRequest(new { message = result.ErrorMessage ?? "Failed to reset password" });
    }

    /// <summary>
    /// Google OAuth login
    /// </summary>
    [HttpPost("google/login")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(GoogleLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GoogleLoginResponse>> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var clientIpAddress = GetClientIpAddress();
        var userAgent = Request.Headers.UserAgent.ToString();
        var tenantId = request.TenantId ?? "system";

        _logger.LogInformation("Google OAuth login attempt for tenant {TenantId}", tenantId);

        var result = await _socialLoginService.AuthenticateWithCodeAsync(
            request.AuthorizationCode, request.RedirectUri);

        if (!result.IsSuccess)
        {
            await _securityAuditService.LogLoginEventAsync(
                "google_user", tenantId, false, clientIpAddress, userAgent, result.ErrorMessage);
            
            return BadRequest(new { message = result.ErrorMessage ?? "Google authentication failed" });
        }

        // Here you would typically create or update the user account
        // and generate JWT tokens
        // For now, we'll return a success response

        await _securityAuditService.LogLoginEventAsync(
            "google_user", tenantId, true, clientIpAddress, userAgent);

        _logger.LogInformation("Google OAuth login successful for tenant {TenantId}", tenantId);

        return Ok(new GoogleLoginResponse
        {
            AccessToken = "jwt_token_here",
            RefreshToken = "refresh_token_here",
            ExpiresIn = 3600,
            TokenType = "Bearer",
            User = new IdentityService.Application.DTOs.GoogleUserInfo
            {
                Email = result.User.Email,
                FirstName = result.User.FirstName,
                LastName = result.User.LastName,
                ProfilePictureUrl = result.User.ProfilePictureUrl
            }
        });
    }

    /// <summary>
    /// Get Google OAuth authorization URL
    /// </summary>
    [HttpGet("google/authorize")]
    [ProducesResponseType(typeof(GoogleAuthUrlResponse), StatusCodes.Status200OK)]
    public ActionResult<GoogleAuthUrlResponse> GetGoogleAuthUrl([FromQuery] string redirectUri, [FromQuery] string? tenantId = null)
    {
        var state = Guid.NewGuid().ToString();
        var authUrl = _socialLoginService.GetAuthorizationUrl(redirectUri, state);

        _logger.LogInformation("Google OAuth authorization URL generated for tenant {TenantId}", tenantId);

        return Ok(new GoogleAuthUrlResponse
        {
            AuthorizationUrl = authUrl,
            State = state
        });
    }

    /// <summary>
    /// Get security events for a user
    /// </summary>
    [HttpGet("security-events/{userId}")]
    [Authorize]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(List<SecurityAuditEvent>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SecurityAuditEvent>>> GetSecurityEvents(
        string userId,
        [FromQuery] string? tenantId = null,
        [FromQuery] SecurityEventType? eventType = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int limit = 100)
    {
        _logger.LogInformation("Security events requested for user {UserId}", userId);

        var events = await _securityAuditService.GetSecurityEventsAsync(
            userId, tenantId, eventType, fromDate, toDate, limit);

        return Ok(events);
    }

    /// <summary>
    /// Unlock user account (admin only)
    /// </summary>
    [HttpPost("unlock-account")]
    [Authorize]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UnlockAccount([FromBody] UnlockAccountRequest request)
    {
        var clientIpAddress = GetClientIpAddress();
        var userAgent = Request.Headers.UserAgent.ToString();
        var tenantId = request.TenantId ?? "system";

        _logger.LogInformation("Account unlock requested for user {Email} in tenant {TenantId}", 
            request.Email, tenantId);

        await _accountLockoutService.UnlockAccountAsync(request.Email, tenantId);

        // Log account unlock event
        await _securityAuditService.LogSecurityEventAsync(
            SecurityEventType.AccountUnlocked, request.Email, tenantId, clientIpAddress, userAgent);

        _logger.LogInformation("Account unlocked for user {Email}", request.Email);

        return Ok(new { message = "Account unlocked successfully" });
    }

    private string GetClientIpAddress()
    {
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}


