using MediatR;
using Microsoft.Extensions.Logging;
using CatalogService.Application.DTOs;
using CatalogService.Application.Interfaces;
using Tenancy;

namespace CatalogService.Application.Commands.Hotel;

public record UpdateHotelCommand(
    string Id,
    string Name,
    string Description,
    string Location,
    string Address,
    string City,
    string Country,
    int StarRating,
    decimal PricePerNight,
    int TotalRooms,
    string[]? Amenities,
    string[]? Images,
    double? Latitude,
    double? Longitude,
    string? ContactEmail,
    string? ContactPhone
) : IRequest<HotelDto?>;

public class UpdateHotelCommandHandler : IRequestHandler<UpdateHotelCommand, HotelDto?>
{
    private readonly IHotelRepository _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<UpdateHotelCommandHandler> _logger;

    public UpdateHotelCommandHandler(
        IHotelRepository repository,
        ITenantContext tenantContext,
        ILogger<UpdateHotelCommandHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<HotelDto?> Handle(UpdateHotelCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating hotel {HotelId} for tenant {TenantId}", request.Id, _tenantContext.TenantId);

        var hotel = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (hotel == null)
            return null;

        hotel.UpdateDetails(
            request.Name,
            request.Description,
            request.Location,
            request.Address,
            request.City,
            request.Country,
            request.StarRating,
            request.PricePerNight,
            request.TotalRooms
        );

        if (request.Amenities != null)
            hotel.UpdateAmenities(request.Amenities);

        if (request.Images != null)
            hotel.UpdateImages(request.Images);

        if (request.Latitude.HasValue && request.Longitude.HasValue)
            hotel.SetLocation(request.Latitude.Value, request.Longitude.Value);

        if (!string.IsNullOrEmpty(request.ContactEmail))
            hotel.UpdateContactInfo(request.ContactEmail, request.ContactPhone);

        await _repository.UpdateAsync(hotel, cancellationToken);

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
