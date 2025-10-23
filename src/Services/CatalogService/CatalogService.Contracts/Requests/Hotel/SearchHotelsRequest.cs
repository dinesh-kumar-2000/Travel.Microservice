namespace CatalogService.Contracts.Requests.Hotel;

public class SearchHotelsRequest
{
    public string? SearchTerm { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? HotelCategory { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? MaxPricePerNight { get; set; }
    public int? MinStarRating { get; set; }
    public DateTime? CheckInDate { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public int? Guests { get; set; }
    public string[]? Amenities { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

