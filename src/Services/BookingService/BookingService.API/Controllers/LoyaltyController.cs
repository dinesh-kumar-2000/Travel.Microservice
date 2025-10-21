using BookingService.Application.Commands;
using BookingService.Application.Queries;
using BookingService.Contracts.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.API.Controllers;

[ApiController]
[Route("api/user/loyalty")]
[Authorize]
public class LoyaltyController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<LoyaltyController> _logger;

    public LoyaltyController(IMediator mediator, ILogger<LoyaltyController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get user's loyalty points and tier information
    /// </summary>
    [HttpGet("points")]
    public async Task<ActionResult<LoyaltyPointsDto>> GetPoints()
    {
        var userId = GetUserId();
        var tenantId = GetTenantId();

        var query = new GetLoyaltyPointsQuery(userId, tenantId);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Get points transaction history
    /// </summary>
    [HttpGet("transactions")]
    public async Task<ActionResult<List<LoyaltyTransactionDto>>> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        var userId = GetUserId();
        var tenantId = GetTenantId();

        var query = new GetLoyaltyTransactionsQuery(userId, tenantId, page, limit);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Get available rewards catalog
    /// </summary>
    [HttpGet("rewards")]
    public async Task<ActionResult<List<LoyaltyRewardDto>>> GetRewards([FromQuery] string? category = null)
    {
        var tenantId = GetTenantId();

        var query = new GetLoyaltyRewardsQuery(tenantId, category);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Redeem points for a reward
    /// </summary>
    [HttpPost("redeem")]
    public async Task<ActionResult<RedemptionResponseDto>> RedeemPoints([FromBody] RedeemPointsRequest request)
    {
        var userId = GetUserId();
        var tenantId = GetTenantId();

        var command = new RedeemLoyaltyPointsCommand(
            userId,
            tenantId,
            request.RewardId,
            request.PointsToRedeem
        );

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(result);
    }

    /// <summary>
    /// Get redemption history
    /// </summary>
    [HttpGet("redemptions")]
    public async Task<ActionResult<List<RedemptionDto>>> GetRedemptions()
    {
        var userId = GetUserId();
        var tenantId = GetTenantId();

        var query = new GetRedemptionHistoryQuery(userId, tenantId);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
        return string.IsNullOrEmpty(userId) ? Guid.Empty : Guid.Parse(userId);
    }

    private Guid GetTenantId()
    {
        var tenantId = User.FindFirst("tenantId")?.Value;
        return string.IsNullOrEmpty(tenantId) ? Guid.Empty : Guid.Parse(tenantId);
    }
}

