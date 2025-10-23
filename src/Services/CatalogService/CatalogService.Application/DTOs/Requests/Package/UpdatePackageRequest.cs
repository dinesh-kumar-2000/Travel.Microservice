namespace CatalogService.Application.DTOs.Requests.Package;

public class UpdatePackageRequest
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PackageType { get; set; } = string.Empty;
    public string DestinationId { get; set; } = string.Empty;
    public string HotelId { get; set; } = string.Empty;
    public string FlightId { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Duration { get; set; } // in days
    public string[] Inclusions { get; set; } = Array.Empty<string>();
    public string[] Exclusions { get; set; } = Array.Empty<string>();
    public string[]? Itinerary { get; set; }
    public string[]? Images { get; set; }
    public bool IsActive { get; set; }
}
