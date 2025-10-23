namespace PaymentService.Application.Queries.Refund;

public class GetRefundsQuery
{
    public string UserId { get; set; } = string.Empty;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
