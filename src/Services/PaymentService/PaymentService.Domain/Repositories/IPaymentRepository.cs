using PaymentService.Domain.Entities;
using SharedKernel.Interfaces;

namespace PaymentService.Domain.Repositories;

public interface IPaymentRepository : IRepository<Payment, string>
{
    Task<IEnumerable<Payment>> GetByBookingIdAsync(string bookingId, CancellationToken cancellationToken = default);
    Task<decimal> CalculateRefundAmountAsync(string paymentId, int daysBeforeTravel, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetByTenantAndDateAsync(string tenantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}

