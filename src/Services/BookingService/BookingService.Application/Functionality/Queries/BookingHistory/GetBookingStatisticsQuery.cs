using MediatR;
using BookingService.Application.DTOs.Responses.BookingHistory;

namespace BookingService.Application.Queries.BookingHistory;

public class GetBookingStatisticsQuery : IRequest<BookingStatisticsResponseDto>
{
    public Guid? UserId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
