using MediatR;
using PaymentService.Contracts.Responses.Transaction;

namespace PaymentService.Application.Queries.Transaction;

public class GetTransactionQuery : IRequest<TransactionResponse?>
{
    public Guid TransactionId { get; set; }
}