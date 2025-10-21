using IdentityService.Application.Commands;
using IdentityService.Application.Queries;
using IdentityService.Contracts.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/auth/2fa")]
[Authorize]
public class TwoFactorAuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TwoFactorAuthController> _logger;

    public TwoFactorAuthController(IMediator mediator, ILogger<TwoFactorAuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Generate 2FA secret and QR code for setup
    /// </summary>
    [HttpPost("generate")]
    public async Task<ActionResult<TwoFactorSecretDto>> GenerateSecret()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token");
        }

        var command = new GenerateTwoFactorSecretCommand(Guid.Parse(userId));
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    /// <summary>
    /// Verify 2FA code and enable 2FA for user
    /// </summary>
    [HttpPost("verify")]
    public async Task<ActionResult> VerifyAndEnable([FromBody] VerifyTwoFactorRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token");
        }

        var command = new VerifyAndEnableTwoFactorCommand(
            Guid.Parse(userId),
            request.Secret,
            request.Code
        );

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(new { message = "Two-factor authentication enabled successfully" });
    }

    /// <summary>
    /// Authenticate user with 2FA code
    /// </summary>
    [HttpPost("authenticate")]
    [AllowAnonymous]
    public async Task<ActionResult<TwoFactorAuthResponseDto>> Authenticate(
        [FromBody] TwoFactorAuthRequest request)
    {
        var command = new AuthenticateWithTwoFactorCommand(
            request.UserId,
            request.Code,
            request.IsBackupCode
        );

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return Unauthorized(new { message = result.Message });
        }

        return Ok(new { token = result.Token });
    }

    /// <summary>
    /// Disable 2FA for user
    /// </summary>
    [HttpPost("disable")]
    public async Task<ActionResult> Disable([FromBody] DisableTwoFactorRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token");
        }

        var command = new DisableTwoFactorCommand(Guid.Parse(userId), request.Password);
        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(new { message = "Two-factor authentication disabled successfully" });
    }

    /// <summary>
    /// Get 2FA status for current user
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<TwoFactorStatusDto>> GetStatus()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token");
        }

        var query = new GetTwoFactorStatusQuery(Guid.Parse(userId));
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Regenerate backup codes
    /// </summary>
    [HttpPost("backup-codes/regenerate")]
    public async Task<ActionResult<BackupCodesDto>> RegenerateBackupCodes()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token");
        }

        var command = new RegenerateBackupCodesCommand(Guid.Parse(userId));
        var result = await _mediator.Send(command);

        return Ok(result);
    }
}

