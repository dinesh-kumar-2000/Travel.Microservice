namespace CatalogService.Contracts.Requests.Hotel;

public class UpdateHotelRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string HotelCategory { get; set; } = string.Empty;
    public int StarRating { get; set; }
    public decimal PricePerNight { get; set; }
    public int TotalRooms { get; set; }
    public List<string> Amenities { get; set; } = new();
    public List<string> Images { get; set; } = new();
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string ContactInfo { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

