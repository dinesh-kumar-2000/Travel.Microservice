using MediatR;
using SharedKernel.Models;
using PaymentService.Contracts.Responses.Refund;

namespace PaymentService.Application.Queries.Refund;

public class GetAllRefundsQuery : IRequest<PaginatedResult<RefundResponse>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
}
