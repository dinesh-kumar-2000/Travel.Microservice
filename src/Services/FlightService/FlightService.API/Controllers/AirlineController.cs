using FlightService.Application.Commands.Airline.CreateAirlineCommand;
using FlightService.Application.Commands.Airline.UpdateAirlineCommand;
using FlightService.Application.Commands.Airline.DeleteAirlineCommand;
using FlightService.Application.Queries.Airline.GetAirlineQuery;
using FlightService.Application.Queries.Airline.GetAllAirlinesQuery;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Models;

namespace FlightService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AirlineController : ControllerBase
{
    private readonly IMediator _mediator;

    public AirlineController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<GetAllAirlinesQuery.AirlineDto>>> GetAllAirlines([FromQuery] GetAllAirlinesQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetAirlineQuery.AirlineDto>> GetAirline(Guid id)
    {
        var query = new GetAirlineQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CreateAirlineCommand.AirlineDto>> CreateAirline([FromBody] CreateAirlineCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAirline), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UpdateAirlineCommand.AirlineDto>> UpdateAirline(Guid id, [FromBody] UpdateAirlineCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAirline(Guid id)
    {
        var command = new DeleteAirlineCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
