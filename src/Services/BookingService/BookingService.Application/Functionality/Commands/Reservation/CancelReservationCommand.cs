using MediatR;

namespace BookingService.Application.Commands.Reservation;

public class CancelReservationCommand : IRequest<Unit>
{
    public Guid ReservationId { get; set; }
}
