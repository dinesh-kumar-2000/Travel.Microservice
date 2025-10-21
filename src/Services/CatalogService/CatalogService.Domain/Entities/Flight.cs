using SharedKernel.Models;

namespace CatalogService.Domain.Entities;

public class Flight : TenantEntity<string>
{
    public string FlightNumber { get; private set; } = string.Empty;
    public string Airline { get; private set; } = string.Empty;
    public string DepartureAirport { get; private set; } = string.Empty;
    public string ArrivalAirport { get; private set; } = string.Empty;
    public string DepartureCity { get; private set; } = string.Empty;
    public string ArrivalCity { get; private set; } = string.Empty;
    public string DepartureCountry { get; private set; } = string.Empty;
    public string ArrivalCountry { get; private set; } = string.Empty;
    public DateTime DepartureTime { get; private set; }
    public DateTime ArrivalTime { get; private set; }
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = "USD";
    public int TotalSeats { get; private set; }
    public int AvailableSeats { get; private set; }
    public FlightClass FlightClass { get; private set; } = FlightClass.Economy;
    public FlightStatus Status { get; private set; } = FlightStatus.Scheduled;
    public string? AircraftType { get; private set; }
    public int? BaggageAllowanceKg { get; private set; }
    public bool HasMeal { get; private set; }
    public bool IsRefundable { get; private set; }

    private Flight() { }

    public static Flight Create(string id, string tenantId, string flightNumber, string airline,
        string departureAirport, string arrivalAirport, string departureCity, string arrivalCity,
        string departureCountry, string arrivalCountry, DateTime departureTime, DateTime arrivalTime,
        decimal price, int totalSeats, FlightClass flightClass)
    {
        return new Flight
        {
            Id = id,
            TenantId = tenantId,
            FlightNumber = flightNumber,
            Airline = airline,
            DepartureAirport = departureAirport,
            ArrivalAirport = arrivalAirport,
            DepartureCity = departureCity,
            ArrivalCity = arrivalCity,
            DepartureCountry = departureCountry,
            ArrivalCountry = arrivalCountry,
            DepartureTime = departureTime,
            ArrivalTime = arrivalTime,
            Price = price,
            TotalSeats = totalSeats,
            AvailableSeats = totalSeats,
            FlightClass = flightClass,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdatePrice(decimal newPrice)
    {
        Price = newPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool ReserveSeats(int quantity)
    {
        if (AvailableSeats < quantity) return false;
        AvailableSeats -= quantity;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public void ReleaseSeats(int quantity)
    {
        AvailableSeats = Math.Min(AvailableSeats + quantity, TotalSeats);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(FlightStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = FlightStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum FlightClass
{
    Economy,
    PremiumEconomy,
    Business,
    FirstClass
}

public enum FlightStatus
{
    Scheduled,
    Boarding,
    Departed,
    Arrived,
    Delayed,
    Cancelled
}

