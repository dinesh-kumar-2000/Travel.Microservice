using MediatR;
using BookingService.Contracts.Responses.BookingHistory;

namespace BookingService.Application.Queries.BookingHistory;

public class GetBookingStatisticsQuery : IRequest<BookingStatisticsResponse>
{
    public Guid? UserId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
