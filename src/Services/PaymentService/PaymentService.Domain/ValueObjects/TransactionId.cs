namespace PaymentService.Domain.ValueObjects;

public class TransactionId
{
    public Guid Value { get; private set; }

    private TransactionId() { Value = Guid.Empty; }

    public TransactionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Transaction ID cannot be empty", nameof(value));

        Value = value;
    }

    public static TransactionId New() => new(Guid.NewGuid());

    public static implicit operator Guid(TransactionId transactionId) => transactionId.Value;
    public static implicit operator TransactionId(Guid transactionId) => new(transactionId);

    public override string ToString() => Value.ToString();
    public override bool Equals(object? obj) => obj is TransactionId transactionId && Value == transactionId.Value;
    public override int GetHashCode() => Value.GetHashCode();
}
