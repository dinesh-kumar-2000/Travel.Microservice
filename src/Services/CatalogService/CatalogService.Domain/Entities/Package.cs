using SharedKernel.Models;

namespace CatalogService.Domain.Entities;

public class Package : TenantEntity<string>
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Destination { get; private set; } = string.Empty;
    public int DurationDays { get; private set; }
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = "USD";
    public int MaxCapacity { get; private set; }
    public int AvailableSlots { get; private set; }
    public PackageStatus Status { get; private set; } = PackageStatus.Active;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string[] Inclusions { get; private set; } = Array.Empty<string>();
    public string[] Exclusions { get; private set; } = Array.Empty<string>();

    private Package() { }

    public static Package Create(string id, string tenantId, string name, string description, 
        string destination, int durationDays, decimal price, int maxCapacity, 
        DateTime startDate, DateTime endDate)
    {
        return new Package
        {
            Id = id,
            TenantId = tenantId,
            Name = name,
            Description = description,
            Destination = destination,
            DurationDays = durationDays,
            Price = price,
            MaxCapacity = maxCapacity,
            AvailableSlots = maxCapacity,
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

    public bool ReserveSlots(int quantity)
    {
        if (AvailableSlots < quantity) return false;
        AvailableSlots -= quantity;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public void ReleaseSlots(int quantity)
    {
        AvailableSlots = Math.Min(AvailableSlots + quantity, MaxCapacity);
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum PackageStatus
{
    Draft,
    Active,
    Inactive,
    Archived
}

