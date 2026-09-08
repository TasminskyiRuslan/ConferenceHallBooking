using ConferenceHallBooking.Domain.Exceptions;

namespace ConferenceHallBooking.Domain.Entities;

/// <summary>
/// Represents a confirmed booking of a conference hall for a specific time slot.
/// Captures the total price at creation time including hall cost and selected services.
/// </summary>
public class Booking
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; private set; }

    /// <summary>Foreign key to the booked hall.</summary>
    public Guid HallId { get; private set; }

    /// <summary>Navigation property to the booked hall.</summary>
    public Hall Hall { get; private set; } = null!;

    /// <summary>Booking start date and time (inclusive).</summary>
    public DateTimeOffset StartTime { get; private set; }

    /// <summary>Booking end date and time (exclusive).</summary>
    public DateTimeOffset EndTime { get; private set; }

    /// <summary>Total cost of the booking (hall + services) at the time of creation.</summary>
    public decimal TotalPrice { get; private set; }

    /// <summary>UTC timestamp when the booking was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private readonly List<BookingOption> _bookingOptions = [];
    /// <summary>Services selected for this booking with their prices frozen at booking time.</summary>
    public IReadOnlyCollection<BookingOption> BookingOptions => _bookingOptions;

    private Booking() { }

    /// <summary>
    /// Creates a new booking. Validates that endTime > startTime and totalPrice > 0.
    /// </summary>
    /// <param name="hallId">ID of the hall to book.</param>
    /// <param name="startTime">Booking start (inclusive).</param>
    /// <param name="endTime">Booking end (exclusive).</param>
    /// <param name="totalPrice">Total cost including hall and services.</param>
    /// <param name="options">Optional service selections with frozen prices.</param>
    public Booking(
        Guid hallId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        decimal totalPrice,
        IEnumerable<BookingOption>? options = null)
    {
        if (endTime <= startTime)
            throw new InvalidBookingTimeException(startTime, endTime);

        if (totalPrice <= 0)
            throw new InvalidEntityFieldException(nameof(Booking), nameof(TotalPrice), "total price must be greater than zero");

        Id = Guid.NewGuid();
        HallId = hallId;
        StartTime = startTime;
        EndTime = endTime;
        TotalPrice = totalPrice;
        CreatedAtUtc = DateTimeOffset.UtcNow;

        if (options is not null)
        {
            _bookingOptions.AddRange(options);
        }
    }
}
