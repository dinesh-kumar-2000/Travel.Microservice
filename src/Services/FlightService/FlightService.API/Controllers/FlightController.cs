using FlightService.Application.Commands.Flight.CreateFlightCommand;
using FlightService.Application.Commands.Flight.UpdateFlightCommand;
using FlightService.Application.Commands.Flight.DeleteFlightCommand;
using FlightService.Application.Queries.Flight.GetFlightQuery;
using FlightService.Application.Queries.Flight.GetAllFlightsQuery;
using FlightService.Application.Queries.Flight.SearchFlightsQuery;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Models;

namespace FlightService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FlightController : ControllerBase
{
    private readonly IMediator _mediator;

    public FlightController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<GetAllFlightsQuery.FlightDto>>> GetAllFlights([FromQuery] GetAllFlightsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetFlightQuery.FlightDto>> GetFlight(Guid id)
    {
        var query = new GetFlightQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("search")]
    public async Task<ActionResult<IEnumerable<SearchFlightsQuery.FlightDto>>> SearchFlights([FromBody] SearchFlightsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CreateFlightCommand.FlightDto>> CreateFlight([FromBody] CreateFlightCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetFlight), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UpdateFlightCommand.FlightDto>> UpdateFlight(Guid id, [FromBody] UpdateFlightCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteFlight(Guid id)
    {
        var command = new DeleteFlightCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
