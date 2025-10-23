using MediatR;
using SharedKernel.Models;
using PaymentService.Contracts.Responses.PaymentMethod;

namespace PaymentService.Application.Queries.PaymentMethod;

public class GetAllPaymentMethodsQuery : IRequest<PaginatedResult<PaymentMethodResponse>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
}
