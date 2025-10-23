using SharedKernel.Models;

namespace FlightService.Domain.Entities;

public class FlightRoute : BaseEntity
{
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string OriginCode { get; set; } = string.Empty;
    public string DestinationCode { get; set; } = string.Empty;
    public decimal Distance { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public virtual ICollection<Flight> Flights { get; set; } = new List<Flight>();
}
