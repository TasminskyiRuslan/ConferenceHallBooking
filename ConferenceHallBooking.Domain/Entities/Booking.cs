namespace ConferenceHallBooking.Domain.Entities;

public class Booking
{
    public Guid Id { get; private set; }

    public Guid HallId { get; private set; }
    public Hall Hall { get; private set; } = null!;

    public DateTimeOffset StartTime { get; private set; }
    public DateTimeOffset EndTime { get; private set; }

    public decimal TotalPrice { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private readonly List<BookingOption> _bookingOptions = new();
    public IReadOnlyCollection<BookingOption> BookingOptions => _bookingOptions.AsReadOnly();

    private Booking() { }

    public Booking(
        Guid hallId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        decimal totalPrice,
        IEnumerable<BookingOption>? options = null)
    {
        if (endTime <= startTime)
            throw new ArgumentException("EndTime must be strictly greater than StartTime.");

        if (totalPrice < 0)
            throw new ArgumentException("TotalPrice cannot be negative.", nameof(totalPrice));

        Id = Guid.NewGuid();
        HallId = hallId;
        StartTime = startTime;
        EndTime = endTime;
        TotalPrice = totalPrice;
        CreatedAtUtc = DateTimeOffset.UtcNow;

        if (options != null)
        {
            _bookingOptions.AddRange(options);
        }
    }
}