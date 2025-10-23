using BookingService.Application.Commands;
using BookingService.Application.Queries;
using BookingService.Application.DTOs;
using Identity.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SharedKernel.Auditing;
using SharedKernel.Models;

namespace BookingService.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(
        IMediator mediator,
        ICurrentUserService currentUser,
        IAuditService auditService,
        ILogger<BookingsController> logger)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Get all bookings with pagination and filters
    /// </summary>
    [HttpGet]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(PagedBookingsResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedBookingsResponseDto>> GetAll(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Getting all bookings for tenant {TenantId}, status {Status}, page {Page}, size {PageSize}",
            _currentUser.TenantId, status ?? "All", page, pageSize);

        var query = new GetBookingsQuery(
            _currentUser.TenantId!,
            null, // Don't filter by customer for admin view
            status,
            page,
            pageSize
        );

        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Create a new booking
    /// </summary>
    [HttpPost]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(CreateBookingResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateBookingResponseDto>> Create([FromBody] CreateBookingRequestDto request)
    {
        _logger.LogInformation("Creating booking for customer {CustomerId} in tenant {TenantId}",
            _currentUser.UserId, _currentUser.TenantId);

        var command = new CreateBookingCommand(
            _currentUser.UserId!,
            _currentUser.TenantId!,
            request.PackageId,
            request.TravelDate,
            request.NumberOfTravelers,
            1000.00m, // This should come from package price calculation
            request.IdempotencyKey
        );

        var result = await _mediator.Send(command);

        // Audit log
        await _auditService.LogAsync(new AuditEntry
        {
            TenantId = _currentUser.TenantId!,
            UserId = _currentUser.UserId!,
            Action = "Create",
            EntityType = "Booking",
            EntityId = result.BookingId,
            NewValues = System.Text.Json.JsonSerializer.Serialize(result),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
        });

        return CreatedAtAction(nameof(GetById), new { id = result.BookingId }, result);
    }

    /// <summary>
    /// Get booking by ID
    /// </summary>
    [HttpGet("{id}")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingDto>> GetById(string id)
    {
        _logger.LogInformation("Getting booking {BookingId}", id);

        var query = new GetBookingByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(new { message = "Booking not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Get my bookings
    /// </summary>
    [HttpGet("my-bookings")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(PagedBookingsResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedBookingsResponseDto>> GetMyBookings(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Getting bookings for customer {CustomerId}", _currentUser.UserId);

        var query = new GetBookingsQuery(
            _currentUser.TenantId!,
            _currentUser.UserId, // Filter by current user
            null,
            page,
            pageSize
        );

        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Confirm a booking (after successful payment)
    /// </summary>
    [HttpPost("{id}/confirm")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(ConfirmBookingResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConfirmBookingResponseDto>> Confirm(
        string id,
        [FromBody] ConfirmBookingRequestDto request)
    {
        _logger.LogInformation("Confirming booking {BookingId} with payment {PaymentId}",
            id, request.PaymentId);

        var command = new ConfirmBookingCommand(id, request.PaymentId);
        var result = await _mediator.Send(command);

        // Audit log
        await _auditService.LogAsync(new AuditEntry
        {
            TenantId = _currentUser.TenantId!,
            UserId = _currentUser.UserId!,
            Action = "Confirm",
            EntityType = "Booking",
            EntityId = id,
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { PaymentId = request.PaymentId, Status = "Confirmed" }),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
        });

        _logger.LogInformation("Booking {BookingId} confirmed successfully", id);

        return Ok(result);
    }

    /// <summary>
    /// Cancel a booking
    /// </summary>
    [HttpPost("{id}/cancel")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(CancelBookingResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CancelBookingResponseDto>> Cancel(
        string id,
        [FromBody] BookingCancelRequestDto? request = null)
    {
        _logger.LogInformation("Cancelling booking {BookingId}", id);

        var command = new UserCancelBookingCommand(id, request?.Reason);
        var result = await _mediator.Send(command);

        // Audit log
        await _auditService.LogAsync(new AuditEntry
        {
            TenantId = _currentUser.TenantId!,
            UserId = _currentUser.UserId!,
            Action = "Cancel",
            EntityType = "Booking",
            EntityId = id,
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { Status = "Cancelled", Reason = request?.Reason }),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
        });

        _logger.LogInformation("Booking {BookingId} cancelled successfully", id);

        return Ok(result);
    }
}

