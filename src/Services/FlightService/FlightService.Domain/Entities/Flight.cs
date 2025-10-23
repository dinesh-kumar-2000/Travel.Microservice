using SharedKernel.Models;

namespace FlightService.Domain.Entities;

public class Flight : BaseEntity
{
    public string FlightNumber { get; set; } = string.Empty;
    public Guid AirlineId { get; set; }
    public Guid RouteId { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public decimal Price { get; set; }
    public int AvailableSeats { get; set; }
    public int TotalSeats { get; set; }
    public string AircraftType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    
    // Navigation properties
    public virtual Airline Airline { get; set; } = null!;
    public virtual FlightRoute Route { get; set; } = null!;
}
