using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CatalogService.Contracts.Responses.Hotel;
using CatalogService.Contracts.Requests.Hotel;
using CatalogService.Application.Commands.Hotel;
using CatalogService.Application.Queries.Hotel;
using MediatR;
using SharedKernel.Models;

namespace CatalogService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HotelController : ControllerBase
{
    private readonly IMediator _mediator;

    public HotelController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<HotelResponse>>> GetAllHotels(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        var query = new GetAllHotelsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HotelResponse>> GetHotel(Guid id)
    {
        var query = new GetHotelQuery { HotelId = id };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    [HttpPost("search")]
    public async Task<ActionResult<PaginatedResult<HotelResponse>>> SearchHotels(
        [FromBody] SearchHotelsRequest request)
    {
        var query = new SearchHotelsQuery
        {
            SearchTerm = request.SearchTerm,
            City = request.City,
            Country = request.Country,
            HotelCategory = request.HotelCategory,
            MinPrice = request.MinPrice,
            MaxPrice = request.MaxPrice,
            CheckInDate = request.CheckInDate,
            CheckOutDate = request.CheckOutDate,
            Guests = request.Guests,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<ActionResult<HotelResponse>> CreateHotel(
        [FromBody] CreateHotelRequest request)
    {
        var command = new CreateHotelCommand
        {
            Name = request.Name,
            Description = request.Description,
            Address = request.Address,
            City = request.City,
            Country = request.Country,
            HotelCategory = request.HotelCategory,
            StarRating = request.StarRating,
            PricePerNight = request.PricePerNight,
            Amenities = request.Amenities?.ToArray(),
            Images = request.Images?.ToArray(),
            ContactInfo = request.ContactInfo
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetHotel), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<ActionResult<HotelResponse>> UpdateHotel(
        Guid id, 
        [FromBody] UpdateHotelRequest request)
    {
        var command = new UpdateHotelCommand(
            id.ToString(),
            request.Name,
            request.Description,
            request.Location ?? string.Empty,
            request.Address,
            request.City,
            request.Country,
            request.StarRating,
            request.PricePerNight,
            request.TotalRooms,
            request.Amenities?.ToArray(),
            request.Images?.ToArray(),
            request.Latitude,
            request.Longitude,
            request.ContactEmail,
            request.ContactPhone
        );

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<IActionResult> DeleteHotel(Guid id)
    {
        var command = new DeleteHotelCommand(id.ToString());
        await _mediator.Send(command);
        return NoContent();
    }
}
