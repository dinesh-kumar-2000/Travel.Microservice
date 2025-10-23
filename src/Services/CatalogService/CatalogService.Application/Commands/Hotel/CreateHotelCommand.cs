using MediatR;
using Microsoft.Extensions.Logging;
using CatalogService.Contracts.DTOs;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Repositories;
using SharedKernel.Utilities;
using Tenancy;

namespace CatalogService.Application.Commands.Hotel;

public class CreateHotelCommand : IRequest<HotelDto>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? HotelCategory { get; set; }
    public int StarRating { get; set; }
    public decimal PricePerNight { get; set; }
    public int TotalRooms { get; set; }
    public string[]? Amenities { get; set; }
    public string[]? Images { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactInfo { get; set; }
}

public class CreateHotelCommandHandler : IRequestHandler<CreateHotelCommand, HotelDto>
{
    private readonly IHotelRepository _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<CreateHotelCommandHandler> _logger;

    public CreateHotelCommandHandler(
        IHotelRepository repository,
        ITenantContext tenantContext,
        ILogger<CreateHotelCommandHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<HotelDto> Handle(CreateHotelCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating hotel {HotelName} for tenant {TenantId}", 
            request.Name, _tenantContext.TenantId);

        var hotelId = UlidGenerator.Generate();
        var hotel = Domain.Entities.Hotel.Create(
            hotelId,
            _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context required"),
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

        await _repository.AddAsync(hotel, cancellationToken);

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

