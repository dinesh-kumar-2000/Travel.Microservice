using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CatalogService.Application.DTOs.Responses.Destination;
using CatalogService.Application.DTOs.Requests.Destination;
using CatalogService.Application.Commands.Destination;
using CatalogService.Application.Queries.Destination;
using MediatR;
using SharedKernel.Models;

namespace CatalogService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DestinationController : ControllerBase
{
    private readonly IMediator _mediator;

    public DestinationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<DestinationResponse>>> GetAllDestinations(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        var query = new GetAllDestinationsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DestinationResponse>> GetDestination(Guid id)
    {
        var query = new GetDestinationQuery { DestinationId = id };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    [HttpPost("search")]
    public async Task<ActionResult<PaginatedResult<DestinationResponse>>> SearchDestinations(
        [FromBody] SearchDestinationsRequest request)
    {
        var query = new SearchDestinationsQuery
        {
            SearchTerm = request.SearchTerm,
            DestinationType = request.DestinationType,
            Country = request.Country,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<ActionResult<DestinationResponse>> CreateDestination(
        [FromBody] CreateDestinationRequest request)
    {
        var command = new CreateDestinationCommand
        {
            Name = request.Name,
            Description = request.Description,
            Country = request.Country,
            City = request.City,
            DestinationType = request.DestinationType,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Images = request.Images?.ToList(),
            Attractions = request.Attractions?.ToList(),
            BestTimeToVisit = request.BestTimeToVisit,
            Climate = request.Climate
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetDestination), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<ActionResult<DestinationResponse>> UpdateDestination(
        Guid id, 
        [FromBody] UpdateDestinationRequest request)
    {
        var command = new UpdateDestinationCommand
        {
            DestinationId = id,
            Name = request.Name,
            Description = request.Description,
            Country = request.Country,
            City = request.City,
            DestinationType = request.DestinationType,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Images = request.Images?.ToList(),
            Attractions = request.Attractions?.ToList(),
            BestTimeToVisit = request.BestTimeToVisit,
            Climate = request.Climate
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<IActionResult> DeleteDestination(Guid id)
    {
        var command = new DeleteDestinationCommand { DestinationId = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
