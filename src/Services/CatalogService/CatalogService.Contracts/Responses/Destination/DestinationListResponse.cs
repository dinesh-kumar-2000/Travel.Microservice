using CatalogService.Contracts.Responses.Destination;

namespace CatalogService.Contracts.Responses.Destination;

public class DestinationListResponse
{
    public List<DestinationResponse> Destinations { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
