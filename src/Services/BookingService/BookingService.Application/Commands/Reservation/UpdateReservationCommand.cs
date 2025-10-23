using MediatR;
using BookingService.Contracts.Responses.Reservation;

namespace BookingService.Application.Commands.Reservation;

public class UpdateReservationCommand : IRequest<ReservationResponse>
{
    public Guid ReservationId { get; set; }
    public int ServiceType { get; set; }
    public Guid ServiceId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime ReservationDate { get; set; }
    public DateTime? CheckInDate { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public string? SpecialRequests { get; set; }
}
