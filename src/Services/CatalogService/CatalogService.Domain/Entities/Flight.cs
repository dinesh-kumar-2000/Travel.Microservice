namespace CatalogService.Domain.Entities;

public class Flight
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public string Airline { get; set; } = string.Empty;
    public string AircraftType { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public string Duration { get; set; } = string.Empty;
    public string Status { get; set; } = "active";

    // Pricing
    public decimal EconomyPrice { get; set; }
    public decimal BusinessPrice { get; set; }
    public decimal FirstClassPrice { get; set; }

    // Seats
    public int EconomySeats { get; set; }
    public int BusinessSeats { get; set; }
    public int FirstClassSeats { get; set; }
    public int EconomyAvailable { get; set; }
    public int BusinessAvailable { get; set; }
    public int FirstClassAvailable { get; set; }

    // Amenities
    public string? BaggageAllowance { get; set; }
    public bool Meals { get; set; }
    public bool Wifi { get; set; }
    public string? Layovers { get; set; }
    public string? Notes { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }

    public void UpdateAvailableSeats(string seatClass, int count)
    {
        switch (seatClass.ToLower())
        {
            case "economy":
                EconomyAvailable = Math.Max(0, EconomyAvailable - count);
                break;
            case "business":
                BusinessAvailable = Math.Max(0, BusinessAvailable - count);
                break;
            case "first_class":
            case "firstclass":
                FirstClassAvailable = Math.Max(0, FirstClassAvailable - count);
                break;
        }
    }

    public bool HasAvailableSeats(string seatClass, int requiredCount)
    {
        return seatClass.ToLower() switch
        {
            "economy" => EconomyAvailable >= requiredCount,
            "business" => BusinessAvailable >= requiredCount,
            "first_class" or "firstclass" => FirstClassAvailable >= requiredCount,
            _ => false
        };
    }

    public decimal GetPrice(string seatClass)
    {
        return seatClass.ToLower() switch
        {
            "economy" => EconomyPrice,
            "business" => BusinessPrice,
            "first_class" or "firstclass" => FirstClassPrice,
            _ => 0
        };
    }
}
