using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BookingService.Contracts.Responses.Reservation;
using BookingService.Contracts.Requests.Reservation;
using BookingService.Application.Commands.Reservation;
using BookingService.Application.Queries.Reservation;
using MediatR;
using SharedKernel.Models;

namespace BookingService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservationController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReservationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<ReservationResponse>>> GetAllReservations(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        var query = new GetAllReservationsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReservationResponse>> GetReservation(Guid id)
    {
        var query = new GetReservationQuery { ReservationId = id };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<ActionResult<ReservationResponse>> CreateReservation(
        [FromBody] CreateReservationRequest request)
    {
        var command = new CreateReservationCommand
        {
            BookingId = request.BookingId,
            ServiceType = request.ServiceType,
            ServiceId = request.ServiceId,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            TotalPrice = request.TotalPrice,
            ReservationDate = request.ReservationDate,
            CheckInDate = request.CheckInDate,
            CheckOutDate = request.CheckOutDate,
            SpecialRequests = request.SpecialRequests
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetReservation), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<ActionResult<ReservationResponse>> UpdateReservation(
        Guid id, 
        [FromBody] UpdateReservationRequest request)
    {
        var command = new UpdateReservationCommand
        {
            ReservationId = id,
            ServiceType = request.ServiceType,
            ServiceId = request.ServiceId,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            TotalPrice = request.TotalPrice,
            ReservationDate = request.ReservationDate,
            CheckInDate = request.CheckInDate,
            CheckOutDate = request.CheckOutDate,
            SpecialRequests = request.SpecialRequests
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<IActionResult> CancelReservation(Guid id)
    {
        var command = new CancelReservationCommand { ReservationId = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
