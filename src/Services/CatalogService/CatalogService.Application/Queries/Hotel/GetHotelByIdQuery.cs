using MediatR;
using Microsoft.Extensions.Logging;
using CatalogService.Contracts.DTOs;
using CatalogService.Domain.Repositories;

namespace CatalogService.Application.Queries.Hotel;

public record GetHotelByIdQuery(string Id) : IRequest<HotelDto?>;

public class GetHotelByIdQueryHandler : IRequestHandler<GetHotelByIdQuery, HotelDto?>
{
    private readonly IHotelRepository _repository;
    private readonly ILogger<GetHotelByIdQueryHandler> _logger;

    public GetHotelByIdQueryHandler(
        IHotelRepository repository,
        ILogger<GetHotelByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<HotelDto?> Handle(GetHotelByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting hotel {HotelId}", request.Id);

        var hotel = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (hotel == null)
            return null;

        return new HotelDto(
            hotel.Id,
            hotel.TenantId,
            hotel.Name,
            hotel.Description,
            hotel.Location,
            hotel.Address,
            hotel.City,
            hotel.Country,
            hotel.StarRating,
            hotel.PricePerNight,
            hotel.Currency,
            hotel.TotalRooms,
            hotel.AvailableRooms,
            hotel.Status.ToString(),
            hotel.Amenities,
            hotel.Images,
            hotel.Latitude,
            hotel.Longitude,
            hotel.ContactEmail,
            hotel.ContactPhone,
            hotel.CreatedAt
        );
    }
}

