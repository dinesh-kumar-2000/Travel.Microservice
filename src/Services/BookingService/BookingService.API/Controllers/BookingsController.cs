using BookingService.Application.Commands;
using BookingService.Contracts.DTOs;
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
    /// Get all bookings with pagination
    /// </summary>
    [HttpGet]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BookingDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<BookingDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Getting all bookings for tenant {TenantId}, page {PageNumber}, size {PageSize}",
            _currentUser.TenantId, pageNumber, pageSize);

        // For now, return empty results. This should use a query handler in a real implementation
        var emptyResult = new PagedResult<BookingDto>(
            Array.Empty<BookingDto>(),
            0,
            pageNumber,
            pageSize
        );

        return Ok(ApiResponse<PagedResult<BookingDto>>.SuccessResponse(emptyResult));
    }

    /// <summary>
    /// Create a new booking
    /// </summary>
    [HttpPost]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(CreateBookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateBookingResponse>> Create([FromBody] CreateBookingRequest request)
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
        // This would use a query handler
        return Ok(new BookingDto(
            id,
            "BK20251015001",
            "package123",
            DateTime.UtcNow.AddDays(30),
            2,
            1000.00m,
            "Pending"
        ));
    }

    /// <summary>
    /// Get my bookings
    /// </summary>
    [HttpGet("my-bookings")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(IEnumerable<BookingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BookingDto>>> GetMyBookings()
    {
        _logger.LogInformation("Getting bookings for customer {CustomerId}", _currentUser.UserId);

        // This would use a query handler
        return Ok(Array.Empty<BookingDto>());
    }
}

