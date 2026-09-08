namespace ConferenceHallBooking.Domain.Exceptions;

public class HallHasBookingsException(Guid hallId, int bookingCount)
    : BusinessRuleException($"Hall '{hallId}' has {bookingCount} booking(s) and cannot be deleted.", "HALL_HAS_BOOKINGS")
{
    public Guid HallId { get; } = hallId;
    public int BookingCount { get; } = bookingCount;
}
