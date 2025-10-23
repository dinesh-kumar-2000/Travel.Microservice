using MediatR;
using PaymentService.Application.DTOs.Responses.Transaction;

namespace PaymentService.Application.Queries.Transaction;

public class GetTransactionQuery : IRequest<TransactionResponse?>
{
    public Guid TransactionId { get; set; }
}