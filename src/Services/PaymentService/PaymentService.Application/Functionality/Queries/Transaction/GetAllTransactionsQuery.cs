using MediatR;
using SharedKernel.Models;
using PaymentService.Application.DTOs.Responses.Transaction;

namespace PaymentService.Application.Queries.Transaction;

public class GetAllTransactionsQuery : IRequest<PaginatedResult<TransactionResponse>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
}
