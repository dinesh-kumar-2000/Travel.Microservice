using CatalogService.Application.Commands;
using CatalogService.Application.Queries;
using CatalogService.Contracts.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.API.Controllers;

[ApiController]
[Route("api/tenantadmin/flights")]
[Authorize(Roles = "TenantAdmin")]
public class FlightsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<FlightsController> _logger;

    public FlightsController(IMediator mediator, ILogger<FlightsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all flights for tenant with pagination and filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<FlightDto>>> GetFlights(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null)
    {
        var tenantId = GetTenantId();
        var query = new GetFlightsQuery(tenantId, page, limit, search, status);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Get flight by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<FlightDto>> GetFlight(Guid id)
    {
        var tenantId = GetTenantId();
        var query = new GetFlightByIdQuery(id, tenantId);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(new { message = "Flight not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Create new flight
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<FlightDto>> CreateFlight([FromBody] CreateFlightRequest request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        var command = new CreateFlightCommand(
            tenantId,
            userId,
            request.FlightNumber,
            request.Airline,
            request.AircraftType,
            request.Origin,
            request.Destination,
            request.DepartureTime,
            request.ArrivalTime,
            request.Duration,
            request.EconomyPrice,
            request.BusinessPrice,
            request.FirstClassPrice,
            request.EconomySeats,
            request.BusinessSeats,
            request.FirstClassSeats,
            request.BaggageAllowance,
            request.Meals,
            request.Wifi,
            request.Layovers,
            request.Notes
        );

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetFlight), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update flight
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<FlightDto>> UpdateFlight(Guid id, [FromBody] UpdateFlightRequest request)
    {
        var tenantId = GetTenantId();

        var command = new UpdateFlightCommand(
            id,
            tenantId,
            request.FlightNumber,
            request.Airline,
            request.AircraftType,
            request.Origin,
            request.Destination,
            request.DepartureTime,
            request.ArrivalTime,
            request.Duration,
            request.EconomyPrice,
            request.BusinessPrice,
            request.FirstClassPrice,
            request.EconomySeats,
            request.BusinessSeats,
            request.FirstClassSeats,
            request.BaggageAllowance,
            request.Meals,
            request.Wifi,
            request.Layovers,
            request.Notes,
            request.Status
        );

        var result = await _mediator.Send(command);
        
        if (result == null)
        {
            return NotFound(new { message = "Flight not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete flight
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteFlight(Guid id)
    {
        var tenantId = GetTenantId();
        var command = new DeleteFlightCommand(id, tenantId);
        var result = await _mediator.Send(command);

        if (!result)
        {
            return NotFound(new { message = "Flight not found" });
        }

        return NoContent();
    }

    private Guid GetTenantId()
    {
        var tenantId = User.FindFirst("tenantId")?.Value;
        return string.IsNullOrEmpty(tenantId) ? Guid.Empty : Guid.Parse(tenantId);
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
        return string.IsNullOrEmpty(userId) ? Guid.Empty : Guid.Parse(userId);
    }
}
