namespace BookingService.Contracts.Responses.Reservation;

public class ReservationResponse
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public int ServiceType { get; set; }
    public Guid ServiceId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime ReservationDate { get; set; }
    public DateTime? CheckInDate { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public string? SpecialRequests { get; set; }
    public int Status { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
