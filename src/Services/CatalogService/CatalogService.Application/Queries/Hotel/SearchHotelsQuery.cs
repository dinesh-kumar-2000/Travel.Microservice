using MediatR;
using Microsoft.Extensions.Logging;
using CatalogService.Contracts.DTOs;
using CatalogService.Domain.Repositories;
using Tenancy;

namespace CatalogService.Application.Queries.Hotel;

public record SearchHotelsQuery(
    string? City,
    string? Country,
    decimal? MinPrice,
    decimal? MaxPrice,
    int? StarRating,
    string[]? Amenities,
    int Page,
    int PageSize
) : IRequest<PagedHotelsResponse>;

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
            request.StarRating,
            request.Amenities,
            request.Page,
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
            request.Page,
            request.PageSize,
            totalPages
        );
    }
}

