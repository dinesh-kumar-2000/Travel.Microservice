namespace BookingService.Domain.ValueObjects;

public class BookingReference
{
    public string Value { get; private set; }

    private BookingReference() { Value = string.Empty; }

    public BookingReference(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Booking reference cannot be null or empty", nameof(value));

        if (value.Length < 6 || value.Length > 20)
            throw new ArgumentException("Booking reference must be between 6 and 20 characters", nameof(value));

        Value = value.ToUpperInvariant();
    }

    public static BookingReference New()
    {
        var prefix = "BK";
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = new Random().Next(1000, 9999);
        return new($"{prefix}{timestamp}{random}");
    }

    public static implicit operator string(BookingReference bookingReference) => bookingReference.Value;
    public static implicit operator BookingReference(string bookingReference) => new(bookingReference);

    public override string ToString() => Value;
    public override bool Equals(object? obj) => obj is BookingReference bookingReference && Value == bookingReference.Value;
    public override int GetHashCode() => Value.GetHashCode();
}
