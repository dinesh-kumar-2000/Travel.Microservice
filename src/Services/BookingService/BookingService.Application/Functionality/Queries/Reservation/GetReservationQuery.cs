using MediatR;
using BookingService.Application.DTOs.Responses.Reservation;

namespace BookingService.Application.Queries.Reservation;

public class GetReservationQuery : IRequest<ReservationResponseDto?>
{
    public Guid ReservationId { get; set; }
}
