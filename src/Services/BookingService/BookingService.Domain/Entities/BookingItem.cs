using SharedKernel.Models;

namespace BookingService.Domain.Entities;

public class BookingItem : BaseEntity<string>
{
    public Guid BookingId { get; set; }
    public int ServiceType { get; set; }
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime? ServiceDate { get; set; }
    public string? SpecialInstructions { get; set; }
    public bool IsActive { get; set; } = true;
}
