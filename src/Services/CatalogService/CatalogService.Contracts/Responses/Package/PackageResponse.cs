namespace CatalogService.Contracts.Responses.Package;

public class PackageResponse
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PackageType { get; set; } = string.Empty;
    public string DestinationId { get; set; } = string.Empty;
    public string HotelId { get; set; } = string.Empty;
    public string FlightId { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int Duration { get; set; } // in days
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? MaxTravelers { get; set; }
    public string[] IncludedServices { get; set; } = Array.Empty<string>();
    public string[] ExcludedServices { get; set; } = Array.Empty<string>();
    public string[]? Itinerary { get; set; }
    public string[]? Images { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
