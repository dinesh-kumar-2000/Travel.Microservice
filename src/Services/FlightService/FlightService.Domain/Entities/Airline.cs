using SharedKernel.Models;

namespace FlightService.Domain.Entities;

public class Airline : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Website { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public virtual ICollection<Flight> Flights { get; set; } = new List<Flight>();
}
