namespace CatalogService.Application.DTOs.Requests.Destination;

public class UpdateDestinationRequest
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int DestinationType { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool IsActive { get; set; }
    public string[]? Images { get; set; }
    public string[]? Attractions { get; set; }
    public string? BestTimeToVisit { get; set; }
    public string? Climate { get; set; }
}