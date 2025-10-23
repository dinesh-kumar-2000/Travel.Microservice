using MediatR;
using Microsoft.Extensions.Logging;
using CatalogService.Application.DTOs.Responses.Hotel;
using CatalogService.Application.Interfaces;
using SharedKernel.Models;
using Tenancy;

namespace CatalogService.Application.Queries.Hotel;

public class GetAllHotelsQuery : IRequest<PaginatedResult<HotelResponse>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
}

public class GetAllHotelsQueryHandler : IRequestHandler<GetAllHotelsQuery, PaginatedResult<HotelResponse>>
{
    private readonly IHotelRepository _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetAllHotelsQueryHandler> _logger;

    public GetAllHotelsQueryHandler(
        IHotelRepository repository,
        ITenantContext tenantContext,
        ILogger<GetAllHotelsQueryHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<PaginatedResult<HotelResponse>> Handle(GetAllHotelsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all hotels for tenant {TenantId}", _tenantContext.TenantId);

        var hotels = await _repository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var hotelResponses = hotels.Items.Select(hotel => new HotelResponse
        {
            Id = hotel.Id,
            TenantId = hotel.TenantId,
            Name = hotel.Name,
            Description = hotel.Description,
            Location = hotel.Location,
            Address = hotel.Address,
            City = hotel.City,
            Country = hotel.Country,
            StarRating = hotel.StarRating,
            PricePerNight = hotel.PricePerNight,
            Currency = hotel.Currency,
            TotalRooms = hotel.TotalRooms,
            AvailableRooms = hotel.AvailableRooms,
            Status = hotel.Status.ToString(),
            Amenities = hotel.Amenities,
            Images = hotel.Images,
            Latitude = hotel.Latitude,
            Longitude = hotel.Longitude,
            ContactEmail = hotel.ContactEmail,
            ContactPhone = hotel.ContactPhone,
            CreatedAt = hotel.CreatedAt
        }).ToList();

        return new PaginatedResult<HotelResponse>
        {
            Items = hotelResponses,
            TotalCount = hotels.TotalCount,
            PageNumber = hotels.PageNumber,
            PageSize = hotels.PageSize
        };
    }
}
