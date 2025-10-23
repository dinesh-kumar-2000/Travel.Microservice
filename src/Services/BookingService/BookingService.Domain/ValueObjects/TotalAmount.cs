namespace BookingService.Domain.ValueObjects;

public class TotalAmount
{
    public decimal Value { get; private set; }
    public string Currency { get; private set; }

    private TotalAmount() 
    { 
        Value = 0;
        Currency = "USD";
    }

    public TotalAmount(decimal value, string currency = "USD")
    {
        if (value < 0)
            throw new ArgumentException("Total amount cannot be negative", nameof(value));

        if (string.IsNullOrEmpty(currency))
            throw new ArgumentException("Currency cannot be null or empty", nameof(currency));

        Value = value;
        Currency = currency.ToUpperInvariant();
    }

    public static TotalAmount Zero(string currency = "USD") => new(0, currency);

    public TotalAmount Add(TotalAmount other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add amounts with different currencies");

        return new TotalAmount(Value + other.Value, Currency);
    }

    public TotalAmount Subtract(TotalAmount other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot subtract amounts with different currencies");

        return new TotalAmount(Value - other.Value, Currency);
    }

    public static implicit operator decimal(TotalAmount totalAmount) => totalAmount.Value;
    public static implicit operator TotalAmount(decimal value) => new(value);

    public override string ToString() => $"{Value:C} {Currency}";
    public override bool Equals(object? obj) => obj is TotalAmount totalAmount && Value == totalAmount.Value && Currency == totalAmount.Currency;
    public override int GetHashCode() => HashCode.Combine(Value, Currency);
}
