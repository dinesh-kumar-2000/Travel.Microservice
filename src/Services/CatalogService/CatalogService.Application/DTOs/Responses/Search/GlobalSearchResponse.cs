using SharedKernel.Models;
using CatalogService.Application.DTOs.Responses.Destination;
using CatalogService.Application.DTOs.Responses.Hotel;
using CatalogService.Application.DTOs.Responses.Package;

namespace CatalogService.Application.DTOs.Responses.Search;

public class GlobalSearchResponse
{
    public PaginatedResult<DestinationResponse> Destinations { get; set; } = PaginatedResult<DestinationResponse>.Empty();
    public PaginatedResult<HotelResponse> Hotels { get; set; } = PaginatedResult<HotelResponse>.Empty();
    public PaginatedResult<PackageResponse> Packages { get; set; } = PaginatedResult<PackageResponse>.Empty();
    public int TotalResults { get; set; }
}
