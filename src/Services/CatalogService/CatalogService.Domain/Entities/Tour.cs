using SharedKernel.Models;

namespace CatalogService.Domain.Entities;

public class Tour : TenantEntity<string>
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Destination { get; private set; } = string.Empty;
    public string[] Locations { get; private set; } = Array.Empty<string>();
    public int DurationDays { get; private set; }
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = "USD";
    public int MaxGroupSize { get; private set; }
    public int AvailableSpots { get; private set; }
    public TourStatus Status { get; private set; } = TourStatus.Active;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string[] Inclusions { get; private set; } = Array.Empty<string>();
    public string[] Exclusions { get; private set; } = Array.Empty<string>();
    public string[] Images { get; private set; } = Array.Empty<string>();
    public DifficultyLevel Difficulty { get; private set; } = DifficultyLevel.Moderate;
    public string[] Languages { get; private set; } = Array.Empty<string>();
    public int MinAge { get; private set; }
    public string? MeetingPoint { get; private set; }
    public string? GuideInfo { get; private set; }

    private Tour() { }

    public static Tour Create(string id, string tenantId, string name, string description,
        string destination, int durationDays, decimal price, int maxGroupSize,
        DateTime startDate, DateTime endDate)
    {
        return new Tour
        {
            Id = id,
            TenantId = tenantId,
            Name = name,
            Description = description,
            Destination = destination,
            DurationDays = durationDays,
            Price = price,
            MaxGroupSize = maxGroupSize,
            AvailableSpots = maxGroupSize,
            StartDate = startDate,
            EndDate = endDate,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdatePrice(decimal newPrice)
    {
        Price = newPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool ReserveSpots(int quantity)
    {
        if (AvailableSpots < quantity) return false;
        AvailableSpots -= quantity;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public void ReleaseSpots(int quantity)
    {
        AvailableSpots = Math.Min(AvailableSpots + quantity, MaxGroupSize);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateInclusions(string[] inclusions)
    {
        Inclusions = inclusions;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateExclusions(string[] exclusions)
    {
        Exclusions = exclusions;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateImages(string[] images)
    {
        Images = images;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateLocations(string[] locations)
    {
        Locations = locations;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = TourStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Status = TourStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum TourStatus
{
    Draft,
    Active,
    Inactive,
    FullyBooked,
    Cancelled,
    Completed
}

public enum DifficultyLevel
{
    Easy,
    Moderate,
    Challenging,
    Difficult,
    Expert
}

