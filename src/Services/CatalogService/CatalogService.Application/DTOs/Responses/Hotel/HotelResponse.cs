namespace CatalogService.Application.DTOs.Responses.Hotel;

public class HotelResponse
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int StarRating { get; set; }
    public decimal PricePerNight { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int TotalRooms { get; set; }
    public int AvailableRooms { get; set; }
    public string Status { get; set; } = string.Empty;
    public string[] Amenities { get; set; } = Array.Empty<string>();
    public string[] Images { get; set; } = Array.Empty<string>();
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
