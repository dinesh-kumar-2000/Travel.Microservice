namespace BookingService.Domain.ValueObjects;

public class BookingId
{
    public Guid Value { get; private set; }

    private BookingId() { Value = Guid.Empty; }

    public BookingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Booking ID cannot be empty", nameof(value));

        Value = value;
    }

    public static BookingId New() => new(Guid.NewGuid());

    public static implicit operator Guid(BookingId bookingId) => bookingId.Value;
    public static implicit operator BookingId(Guid bookingId) => new(bookingId);

    public override string ToString() => Value.ToString();
    public override bool Equals(object? obj) => obj is BookingId bookingId && Value == bookingId.Value;
    public override int GetHashCode() => Value.GetHashCode();
}
