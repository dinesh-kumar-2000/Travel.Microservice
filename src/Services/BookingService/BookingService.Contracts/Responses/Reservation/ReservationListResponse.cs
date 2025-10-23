using BookingService.Contracts.Responses.Reservation;

namespace BookingService.Contracts.Responses.Reservation;

public class ReservationListResponse
{
    public List<ReservationResponse> Reservations { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
