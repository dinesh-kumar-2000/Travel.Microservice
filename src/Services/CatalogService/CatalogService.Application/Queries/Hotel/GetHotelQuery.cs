using MediatR;
using Microsoft.Extensions.Logging;
using CatalogService.Contracts.Responses.Hotel;
using CatalogService.Domain.Repositories;

namespace CatalogService.Application.Queries.Hotel;

public class GetHotelQuery : IRequest<HotelResponse?>
{
    public Guid HotelId { get; set; }
}

public class GetHotelQueryHandler : IRequestHandler<GetHotelQuery, HotelResponse?>
{
    private readonly IHotelRepository _repository;
    private readonly ILogger<GetHotelQueryHandler> _logger;

    public GetHotelQueryHandler(
        IHotelRepository repository,
        ILogger<GetHotelQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<HotelResponse?> Handle(GetHotelQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting hotel {HotelId}", request.HotelId);

        var hotel = await _repository.GetByIdAsync(request.HotelId.ToString(), cancellationToken);

        if (hotel == null)
            return null;

        return new HotelResponse
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
        };
    }
}
