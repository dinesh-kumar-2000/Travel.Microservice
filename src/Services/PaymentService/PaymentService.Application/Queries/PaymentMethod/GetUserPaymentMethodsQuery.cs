using MediatR;
using SharedKernel.Models;
using PaymentService.Contracts.Responses.PaymentMethod;

namespace PaymentService.Application.Queries.PaymentMethod;

public class GetUserPaymentMethodsQuery : IRequest<PaginatedResult<PaymentMethodResponse>>
{
    public Guid UserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
