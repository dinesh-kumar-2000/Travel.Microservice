namespace CatalogService.Application.DTOs.Requests.Hotel;

public class CreateHotelRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string HotelCategory { get; set; } = string.Empty;
    public int StarRating { get; set; }
    public decimal PricePerNight { get; set; }
    public List<string> Amenities { get; set; } = new();
    public List<string> Images { get; set; } = new();
    public string ContactInfo { get; set; } = string.Empty;
}

