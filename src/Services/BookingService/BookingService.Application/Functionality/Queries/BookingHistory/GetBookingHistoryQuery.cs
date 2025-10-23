using MediatR;
using SharedKernel.Models;
using BookingService.Application.DTOs.Responses.BookingHistory;

namespace BookingService.Application.Queries.BookingHistory;

public class GetBookingHistoryQuery : IRequest<PaginatedResult<BookingHistoryResponseDto>>
{
    public Guid UserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
