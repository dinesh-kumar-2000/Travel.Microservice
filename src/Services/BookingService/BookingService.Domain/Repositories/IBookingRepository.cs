using BookingService.Domain.Entities;
using SharedKernel.Interfaces;

namespace BookingService.Domain.Repositories;

public interface IBookingRepository : IRepository<Booking, string>
{
    Task<Booking?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);
    Task<IEnumerable<Booking>> GetByCustomerIdAsync(string customerId, string tenantId, CancellationToken cancellationToken = default);
}

