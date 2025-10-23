using MediatR;
using BookingService.Application.DTOs;

namespace BookingService.Application.Queries;

public record GetBookingsQuery(
    string TenantId,
    string? CustomerId = null,
    string? Status = null,
    int Page = 1,
    int PageSize = 10
) : IRequest<PagedBookingsResponseDto>;

