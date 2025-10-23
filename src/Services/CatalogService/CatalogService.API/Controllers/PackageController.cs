using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CatalogService.Application.DTOs.Responses.Package;
using CatalogService.Application.DTOs.Requests.Package;
using CatalogService.Application.Commands.Package;
using CatalogService.Application.Queries.Package;
using MediatR;
using SharedKernel.Models;

namespace CatalogService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PackageController : ControllerBase
{
    private readonly IMediator _mediator;

    public PackageController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<PackageResponse>>> GetAllPackages(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        var query = new GetAllPackagesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PackageResponse>> GetPackage(Guid id)
    {
        var query = new GetPackageQuery { PackageId = id };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    [HttpPost("search")]
    public async Task<ActionResult<PaginatedResult<PackageResponse>>> SearchPackages(
        [FromBody] SearchPackagesRequest request)
    {
        var query = new SearchPackagesQuery
        {
            SearchTerm = request.SearchTerm,
            PackageType = request.PackageType,
            MinPrice = request.MinPrice,
            MaxPrice = request.MaxPrice,
            Duration = request.Duration,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<ActionResult<PackageResponse>> CreatePackage(
        [FromBody] CreatePackageRequest request)
    {
        var command = new CreatePackageCommand
        {
            Name = request.Name,
            Description = request.Description,
            PackageType = request.PackageType,
            Price = request.Price,
            Duration = request.Duration,
            DestinationId = Guid.TryParse(request.DestinationId, out var destId) ? destId : null,
            HotelId = Guid.TryParse(request.HotelId, out var hotelId) ? hotelId : null,
            FlightId = Guid.TryParse(request.FlightId, out var flightId) ? flightId : null,
            Inclusions = request.Inclusions?.ToList(),
            Exclusions = request.Exclusions?.ToList(),
            Itinerary = request.Itinerary?.FirstOrDefault(),
            Images = request.Images?.ToList()
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetPackage), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<ActionResult<PackageResponse>> UpdatePackage(
        Guid id, 
        [FromBody] UpdatePackageRequest request)
    {
        var command = new UpdatePackageCommand
        {
            PackageId = id,
            Name = request.Name,
            Description = request.Description,
            PackageType = request.PackageType,
            Price = request.Price,
            Duration = request.Duration,
            DestinationId = Guid.TryParse(request.DestinationId, out var destId) ? destId : null,
            HotelId = Guid.TryParse(request.HotelId, out var hotelId) ? hotelId : null,
            FlightId = Guid.TryParse(request.FlightId, out var flightId) ? flightId : null,
            Inclusions = request.Inclusions?.ToList(),
            Exclusions = request.Exclusions?.ToList(),
            Itinerary = request.Itinerary?.FirstOrDefault(),
            Images = request.Images?.ToList()
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<IActionResult> DeletePackage(Guid id)
    {
        var command = new DeletePackageCommand { PackageId = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
