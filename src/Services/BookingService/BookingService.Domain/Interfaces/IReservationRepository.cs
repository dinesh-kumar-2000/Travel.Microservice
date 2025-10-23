using BookingService.Domain.Entities;
using SharedKernel.Interfaces;

namespace BookingService.Domain.Interfaces;

public interface IReservationRepository : IRepository<Reservation, string>
{
    Task<IEnumerable<Reservation>> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reservation>> GetByServiceTypeAsync(int serviceType, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reservation>> GetByServiceIdAsync(Guid serviceId, CancellationToken cancellationToken = default);
    Task<bool> ExistsForServiceAsync(Guid serviceId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
