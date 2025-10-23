using MediatR;
using BookingService.Contracts.Responses.Reservation;

namespace BookingService.Application.Queries.Reservation;

public class GetReservationQuery : IRequest<ReservationResponse?>
{
    public Guid ReservationId { get; set; }
}
