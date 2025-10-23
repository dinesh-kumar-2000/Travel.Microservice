using MediatR;
using Microsoft.Extensions.Logging;
using CatalogService.Contracts.DTOs;
using CatalogService.Domain.Repositories;
using Tenancy;

namespace CatalogService.Application.Queries.Hotel;

public class SearchHotelsQuery : IRequest<PagedHotelsResponse>
{
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? SearchTerm { get; set; }
    public string? HotelCategory { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? MaxPricePerNight { get; set; }
    public int? MinStarRating { get; set; }
    public DateTime? CheckInDate { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public int? Guests { get; set; }
    public string[]? Amenities { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class SearchHotelsQueryHandler : IRequestHandler<SearchHotelsQuery, PagedHotelsResponse>
{
    private readonly IHotelRepository _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<SearchHotelsQueryHandler> _logger;

    public SearchHotelsQueryHandler(
        IHotelRepository repository,
        ITenantContext tenantContext,
        ILogger<SearchHotelsQueryHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<PagedHotelsResponse> Handle(SearchHotelsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Searching hotels for tenant {TenantId}", _tenantContext.TenantId);

        var (hotels, totalCount) = await _repository.SearchHotelsAsync(
            _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context required"),
            request.City,
            request.Country,
            request.MinPrice,
            request.MaxPrice,
            null, // starRating
            null, // amenities
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var hotelDtos = hotels.Select(h => new HotelDto(
            h.Id,
            h.TenantId,
            h.Name,
            h.Description,
            h.Location,
            h.Address,
            h.City,
            h.Country,
            h.StarRating,
            h.PricePerNight,
            h.Currency,
            h.TotalRooms,
            h.AvailableRooms,
            h.Status.ToString(),
            h.Amenities,
            h.Images,
            h.Latitude,
            h.Longitude,
            h.ContactEmail,
            h.ContactPhone,
            h.CreatedAt
        ));

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new PagedHotelsResponse(
            hotelDtos,
            totalCount,
            request.PageNumber,
            request.PageSize,
            totalPages
        );
    }
}

