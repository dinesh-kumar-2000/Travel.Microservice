using FlightService.Application.Commands.FlightRoute.CreateFlightRouteCommand;
using FlightService.Application.Commands.FlightRoute.UpdateFlightRouteCommand;
using FlightService.Application.Commands.FlightRoute.DeleteFlightRouteCommand;
using FlightService.Application.Queries.FlightRoute.GetFlightRouteQuery;
using FlightService.Application.Queries.FlightRoute.GetAllFlightRoutesQuery;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Models;

namespace FlightService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FlightRouteController : ControllerBase
{
    private readonly IMediator _mediator;

    public FlightRouteController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<GetAllFlightRoutesQuery.FlightRouteDto>>> GetAllFlightRoutes([FromQuery] GetAllFlightRoutesQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetFlightRouteQuery.FlightRouteDto>> GetFlightRoute(Guid id)
    {
        var query = new GetFlightRouteQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CreateFlightRouteCommand.FlightRouteDto>> CreateFlightRoute([FromBody] CreateFlightRouteCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetFlightRoute), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UpdateFlightRouteCommand.FlightRouteDto>> UpdateFlightRoute(Guid id, [FromBody] UpdateFlightRouteCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteFlightRoute(Guid id)
    {
        var command = new DeleteFlightRouteCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
