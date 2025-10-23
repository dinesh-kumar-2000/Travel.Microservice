using MediatR;
using BookingService.Application.DTOs;

namespace BookingService.Application.Queries;

public record GetBookingByIdQuery(
    string BookingId
) : IRequest<BookingDto?>;

