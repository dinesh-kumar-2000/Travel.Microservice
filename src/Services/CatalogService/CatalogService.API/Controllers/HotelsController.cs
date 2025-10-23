using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MediatR;
using CatalogService.Contracts.DTOs;
using CatalogService.Application.Commands.Hotel;
using CatalogService.Application.Queries.Hotel;
using SharedKernel.Models;
using Identity.Shared;
using Tenancy;

namespace CatalogService.API.Controllers;

/// <summary>
/// Hotel catalog management
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize(Roles = "TenantAdmin,SuperAdmin")]
public class HotelsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<HotelsController> _logger;

    public HotelsController(
        IMediator mediator,
        ICurrentUserService currentUser,
        ITenantContext tenantContext,
        ILogger<HotelsController> logger)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    /// <summary>
    /// Create a new hotel
    /// </summary>
    [HttpPost]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(HotelDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HotelDto>> Create([FromBody] CreateHotelRequest request)
    {
        _logger.LogInformation("Creating hotel {HotelName} for tenant {TenantId}", 
            request.Name, _tenantContext.TenantId);

        // TODO: Implement command/handler pattern
        // For now, return a mock response
        var hotelDto = new HotelDto(
            Id: Guid.NewGuid().ToString(),
            TenantId: _tenantContext.TenantId ?? "unknown",
            Name: request.Name,
            Description: request.Description,
            Location: request.Location,
            Address: request.Address,
            City: request.City,
            Country: request.Country,
            StarRating: request.StarRating,
            PricePerNight: request.PricePerNight,
            Currency: "USD",
            TotalRooms: request.TotalRooms,
            AvailableRooms: request.TotalRooms,
            Status: "Active",
            Amenities: request.Amenities ?? Array.Empty<string>(),
            Images: request.Images ?? Array.Empty<string>(),
            Latitude: request.Latitude,
            Longitude: request.Longitude,
            ContactEmail: request.ContactEmail,
            ContactPhone: request.ContactPhone,
            CreatedAt: DateTime.UtcNow
        );

        return CreatedAtAction(nameof(GetById), new { id = hotelDto.Id }, hotelDto);
    }

    /// <summary>
    /// Get hotel by ID
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(HotelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HotelDto>> GetById(string id)
    {
        _logger.LogInformation("Getting hotel {HotelId}", id);

        var query = new GetHotelByIdQuery(id);
        var result = await _mediator.Send(query);

        return result == null ? NotFound(new { message = "Hotel not found" }) : Ok(result);
    }

    /// <summary>
    /// Search hotels
    /// </summary>
    [HttpGet("search")]
    [AllowAnonymous]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(PagedHotelsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedHotelsResponse>> Search([FromQuery] SearchHotelsRequest request)
    {
        _logger.LogInformation("Searching hotels - City: {City}, Country: {Country}", 
            request.City, request.Country);

        var query = new SearchHotelsQuery
        {
            City = request.City,
            Country = request.Country,
            MinPrice = request.MinPrice,
            MaxPrice = request.MaxPrice,
            MinStarRating = request.StarRating,
            Amenities = request.Amenities,
            PageNumber = request.Page,
            PageSize = request.PageSize
        };

        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Update hotel
    /// </summary>
    [HttpPut("{id}")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(HotelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HotelDto>> Update(string id, [FromBody] UpdateHotelRequest request)
    {
        _logger.LogInformation("Updating hotel {HotelId}", id);

        // TODO: Implement command/handler pattern
        return NotFound(new { message = "Hotel not found" });
    }

    /// <summary>
    /// Delete hotel
    /// </summary>
    [HttpDelete("{id}")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(string id)
    {
        _logger.LogInformation("Deleting hotel {HotelId}", id);

        // TODO: Implement command/handler pattern
        return NoContent();
    }
}

