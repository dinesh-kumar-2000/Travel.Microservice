using SharedKernel.Models;

namespace CatalogService.Domain.Entities;

public class Hotel : TenantEntity<string>
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Location { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public int StarRating { get; private set; }
    public decimal PricePerNight { get; private set; }
    public string Currency { get; private set; } = "USD";
    public int TotalRooms { get; private set; }
    public int AvailableRooms { get; private set; }
    public HotelStatus Status { get; private set; } = HotelStatus.Active;
    public string[] Amenities { get; private set; } = Array.Empty<string>();
    public string[] Images { get; private set; } = Array.Empty<string>();
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }
    public string? ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }

    private Hotel() { }

    public static Hotel Create(string id, string tenantId, string name, string description,
        string location, string address, string city, string country,
        int starRating, decimal pricePerNight, int totalRooms)
    {
        return new Hotel
        {
            Id = id,
            TenantId = tenantId,
            Name = name,
            Description = description,
            Location = location,
            Address = address,
            City = city,
            Country = country,
            StarRating = starRating,
            PricePerNight = pricePerNight,
            TotalRooms = totalRooms,
            AvailableRooms = totalRooms,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdatePrice(decimal newPrice)
    {
        PricePerNight = newPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool ReserveRooms(int quantity)
    {
        if (AvailableRooms < quantity) return false;
        AvailableRooms -= quantity;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public void ReleaseRooms(int quantity)
    {
        AvailableRooms = Math.Min(AvailableRooms + quantity, TotalRooms);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAmenities(string[] amenities)
    {
        Amenities = amenities;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateImages(string[] images)
    {
        Images = images;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetLocation(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = HotelStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Status = HotelStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum HotelStatus
{
    Draft,
    Active,
    Inactive,
    Archived
}

