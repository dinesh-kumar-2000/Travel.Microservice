namespace BookingService.Application.DTOs.Requests.Reservation;

public class CreateReservationRequestDto
{
    public Guid BookingId { get; set; }
    public int ServiceType { get; set; }
    public Guid ServiceId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime ReservationDate { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public string? SpecialRequests { get; set; }
}
