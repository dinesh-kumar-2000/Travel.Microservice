namespace PaymentService.Domain.Enums;

public enum RefundStatus
{
    Pending = 1,
    Approved = 2,
    Processing = 3,
    Completed = 4,
    Failed = 5,
    Rejected = 6
}
