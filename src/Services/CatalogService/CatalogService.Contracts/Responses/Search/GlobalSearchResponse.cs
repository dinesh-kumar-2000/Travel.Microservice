using SharedKernel.Models;
using CatalogService.Contracts.Responses.Destination;
using CatalogService.Contracts.Responses.Hotel;
using CatalogService.Contracts.Responses.Package;

namespace CatalogService.Contracts.Responses.Search;

public class GlobalSearchResponse
{
    public PaginatedResult<DestinationResponse> Destinations { get; set; } = PaginatedResult<DestinationResponse>.Empty();
    public PaginatedResult<HotelResponse> Hotels { get; set; } = PaginatedResult<HotelResponse>.Empty();
    public PaginatedResult<PackageResponse> Packages { get; set; } = PaginatedResult<PackageResponse>.Empty();
    public int TotalResults { get; set; }
}
