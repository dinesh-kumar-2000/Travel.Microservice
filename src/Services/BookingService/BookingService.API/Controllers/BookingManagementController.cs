using BookingService.Application.Commands;
using BookingService.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.API.Controllers;

[ApiController]
[Route("api/user/bookings")]
[Authorize]
public class BookingManagementController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<BookingManagementController> _logger;

    public BookingManagementController(IMediator mediator, ILogger<BookingManagementController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Modify booking (change dates, passengers, etc.)
    /// </summary>
    [HttpPost("{id}/modify")]
    public async Task<ActionResult<BookingModificationResponseDto>> ModifyBooking(
        Guid id,
        [FromBody] ModifyBookingRequestDto request)
    {
        var userId = GetUserId();

        var command = new ModifyBookingCommand(
            id,
            userId,
            request.NewTravelDate,
            request.NewReturnDate,
            request.NewPassengers,
            request.Reason
        );

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(result);
    }

    /// <summary>
    /// Cancel booking and process refund
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<BookingCancellationResponseDto>> CancelBooking(
        Guid id,
        [FromBody] CancelBookingRequestDto request)
    {
        var userId = GetUserId();

        var command = new CancelBookingCommand(
            id,
            userId,
            request.Reason
        );

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(result);
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
        return string.IsNullOrEmpty(userId) ? Guid.Empty : Guid.Parse(userId);
    }
}

