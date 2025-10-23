using MediatR;
using SharedKernel.Models;
using PaymentService.Application.DTOs.Responses.Transaction;

namespace PaymentService.Application.Queries.Transaction;

public class GetTransactionHistoryQuery : IRequest<PaginatedResult<TransactionResponse>>
{
    public Guid UserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
