using MediatR;
using BookingService.Contracts.DTOs;

namespace BookingService.Application.Queries;

public record GetBookingByIdQuery(
    string BookingId
) : IRequest<BookingDto?>;

