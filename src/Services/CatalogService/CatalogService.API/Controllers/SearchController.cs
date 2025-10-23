using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CatalogService.Application.DTOs.Responses.Destination;
using CatalogService.Application.DTOs.Responses.Hotel;
using CatalogService.Application.DTOs.Responses.Package;
using CatalogService.Application.DTOs.Responses.Search;
using CatalogService.Application.DTOs.Requests.Destination;
using CatalogService.Application.DTOs.Requests.Hotel;
using CatalogService.Application.DTOs.Requests.Package;
using CatalogService.Application.DTOs.Requests.Search;
using CatalogService.Application.DTOs;
using CatalogService.Application.Queries.Destination;
using CatalogService.Application.Queries.Hotel;
using CatalogService.Application.Queries.Package;
using MediatR;
using SharedKernel.Models;

namespace CatalogService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SearchController : ControllerBase
{
    private readonly IMediator _mediator;

    public SearchController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("destinations")]
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

    [HttpPost("hotels")]
    public async Task<ActionResult<PagedHotelsResponse>> SearchHotels(
        [FromBody] CatalogService.Application.DTOs.Requests.Hotel.SearchHotelsRequest request)
    {
        var query = new SearchHotelsQuery
        {
            City = request.City,
            Country = request.Country,
            CheckInDate = request.CheckInDate,
            CheckOutDate = request.CheckOutDate,
            MinStarRating = request.MinStarRating,
            MaxPricePerNight = request.MaxPricePerNight,
            Amenities = request.Amenities?.ToArray(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("packages")]
    public async Task<ActionResult<PaginatedResult<PackageResponse>>> SearchPackages(
        [FromBody] CatalogService.Application.DTOs.Requests.Package.SearchPackagesRequest request)
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

    [HttpPost("global")]
    public async Task<ActionResult<GlobalSearchResponse>> GlobalSearch(
        [FromBody] GlobalSearchRequest request)
    {
        var destinationsTask = Task.Run(async () =>
        {
            var query = new SearchDestinationsQuery
            {
                SearchTerm = request.SearchTerm,
                PageNumber = 1,
                PageSize = 5
            };
            return await _mediator.Send(query);
        });

        var hotelsTask = Task.Run(async () =>
        {
            var query = new SearchHotelsQuery
            {
                City = request.SearchTerm,
                PageNumber = 1,
                PageSize = 5
            };
            var result = await _mediator.Send(query);
            
            // Convert PagedHotelsResponse to PaginatedResult<HotelResponse>
            var hotelResponses = result.Hotels.Select(h => new HotelResponse
            {
                Id = h.Id,
                TenantId = h.TenantId,
                Name = h.Name,
                Description = h.Description,
                Location = h.Location,
                Address = h.Address,
                City = h.City,
                Country = h.Country,
                StarRating = h.StarRating,
                PricePerNight = h.PricePerNight,
                Currency = h.Currency,
                TotalRooms = h.TotalRooms,
                AvailableRooms = h.AvailableRooms,
                Status = h.Status,
                Amenities = h.Amenities,
                Images = h.Images,
                Latitude = h.Latitude,
                Longitude = h.Longitude,
                ContactEmail = h.ContactEmail,
                ContactPhone = h.ContactPhone,
                CreatedAt = h.CreatedAt
            }).ToList();
            
            return new PaginatedResult<HotelResponse>
            {
                Items = hotelResponses,
                TotalCount = result.TotalCount,
                PageNumber = result.Page,
                PageSize = result.PageSize
            };
        });

        var packagesTask = Task.Run(async () =>
        {
            var query = new SearchPackagesQuery
            {
                SearchTerm = request.SearchTerm,
                PageNumber = 1,
                PageSize = 5
            };
            return await _mediator.Send(query);
        });

        await Task.WhenAll(destinationsTask, hotelsTask, packagesTask);

        var result = new GlobalSearchResponse
        {
            Destinations = await destinationsTask,
            Hotels = await hotelsTask,
            Packages = await packagesTask
        };

        return Ok(result);
    }
}
