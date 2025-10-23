using MediatR;
using SharedKernel.Models;
using BookingService.Contracts.Responses.Reservation;

namespace BookingService.Application.Queries.Reservation;

public class GetAllReservationsQuery : IRequest<PaginatedResult<ReservationResponse>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
}
