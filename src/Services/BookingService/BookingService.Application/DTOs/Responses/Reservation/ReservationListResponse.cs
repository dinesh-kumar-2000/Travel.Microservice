using BookingService.Application.DTOs.Responses.Reservation;

namespace BookingService.Application.DTOs.Responses.Reservation;

public class ReservationListResponseDto
{
    public List<ReservationResponseDto> Reservations { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
