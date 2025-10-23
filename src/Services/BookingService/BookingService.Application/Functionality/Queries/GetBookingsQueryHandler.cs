using MediatR;
using Microsoft.Extensions.Logging;
using BookingService.Application.Interfaces;
using BookingService.Application.DTOs;

namespace BookingService.Application.Queries;

public class GetBookingsQueryHandler : IRequestHandler<GetBookingsQuery, PagedBookingsResponseDto>
{
    private readonly IBookingRepository _repository;
    private readonly ILogger<GetBookingsQueryHandler> _logger;

    public GetBookingsQueryHandler(
        IBookingRepository repository,
        ILogger<GetBookingsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PagedBookingsResponseDto> Handle(GetBookingsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting bookings for tenant {TenantId}, customer {CustomerId}, status {Status}, page {Page}", 
            request.TenantId, request.CustomerId ?? "All", request.Status ?? "All", request.Page);

        var (bookings, totalCount) = await _repository.GetPagedBookingsAsync(
            request.TenantId,
            request.CustomerId,
            request.Status,
            request.Page,
            request.PageSize,
            cancellationToken);

        var bookingDtos = bookings.Select(b => new BookingDto(
            b.Id,
            b.TenantId,
            b.CustomerId,
            b.BookingReference,
            b.PackageId,
            b.BookingDate,
            b.TravelDate,
            b.NumberOfTravelers,
            b.TotalAmount,
            b.Currency,
            b.Status.ToString(),
            b.PaymentId,
            b.CreatedAt
        ));

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        _logger.LogInformation("Retrieved {Count} bookings out of {TotalCount}", 
            bookings.Count(), totalCount);

        return new PagedBookingsResponseDto(
            bookingDtos,
            totalCount,
            request.Page,
            request.PageSize,
            totalPages
        );
    }
}

