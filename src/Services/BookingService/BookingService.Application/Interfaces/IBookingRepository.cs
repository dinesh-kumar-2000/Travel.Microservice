using BookingService.Domain.Entities;
using SharedKernel.Interfaces;

namespace BookingService.Application.Interfaces;

public interface IBookingRepository : IRepository<Booking, string>
{
    Task<Booking?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);
    Task<IEnumerable<Booking>> GetByCustomerIdAsync(string customerId, string tenantId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Booking> Bookings, int TotalCount)> GetPagedBookingsAsync(
        string tenantId,
        string? customerId = null,
        string? status = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
    Task<Booking?> GetByReferenceAsync(string bookingReference, CancellationToken cancellationToken = default);
}

