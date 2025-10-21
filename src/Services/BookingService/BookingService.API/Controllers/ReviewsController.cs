using BookingService.Application.Commands;
using BookingService.Application.Queries;
using BookingService.Contracts.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.API.Controllers;

[ApiController]
[Route("api/user/reviews")]
[Authorize]
public class ReviewsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(IMediator mediator, ILogger<ReviewsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get user's reviews
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ReviewDto>>> GetReviews()
    {
        var userId = GetUserId();

        var query = new GetUserReviewsQuery(userId);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Get bookings pending review
    /// </summary>
    [HttpGet("pending")]
    public async Task<ActionResult<List<PendingReviewDto>>> GetPendingReviews()
    {
        var userId = GetUserId();
        var tenantId = GetTenantId();

        var query = new GetPendingReviewsQuery(userId, tenantId);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Submit a review (with optional photos)
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(10_485_760)] // 10 MB limit
    public async Task<ActionResult<ReviewDto>> CreateReview([FromForm] CreateReviewRequest request)
    {
        var userId = GetUserId();
        var tenantId = GetTenantId();

        // Upload photos if any
        List<string> photoUrls = new();
        if (request.Photos != null && request.Photos.Any())
        {
            foreach (var photo in request.Photos)
            {
                // TODO: Upload to blob storage
                // For now, generate placeholder URL
                var photoUrl = $"https://storage.example.com/reviews/{Guid.NewGuid()}.jpg";
                photoUrls.Add(photoUrl);
            }
        }

        var command = new CreateReviewCommand(
            userId,
            tenantId,
            request.BookingId,
            request.ServiceType,
            request.ServiceId,
            request.ServiceName,
            request.Rating,
            request.Title,
            request.Comment,
            photoUrls
        );

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetReviews), new { }, result);
    }

    /// <summary>
    /// Update review
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ReviewDto>> UpdateReview(Guid id, [FromBody] UpdateReviewRequest request)
    {
        var userId = GetUserId();

        var command = new UpdateReviewCommand(
            id,
            userId,
            request.Rating,
            request.Title,
            request.Comment
        );

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Vote on review helpfulness
    /// </summary>
    [HttpPost("{id}/vote")]
    public async Task<ActionResult> VoteReview(Guid id, [FromBody] VoteReviewRequest request)
    {
        var userId = GetUserId();

        var command = new VoteReviewCommand(id, userId, request.IsHelpful);
        await _mediator.Send(command);

        return Ok(new { message = "Vote recorded" });
    }

    /// <summary>
    /// Get reviews for a specific service
    /// </summary>
    [HttpGet("service/{serviceId}")]
    [AllowAnonymous]
    public async Task<ActionResult<ServiceReviewsDto>> GetServiceReviews(
        Guid serviceId,
        [FromQuery] string serviceType,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
    {
        var tenantId = GetTenantId();

        var query = new GetServiceReviewsQuery(tenantId, serviceType, serviceId, page, limit);
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

