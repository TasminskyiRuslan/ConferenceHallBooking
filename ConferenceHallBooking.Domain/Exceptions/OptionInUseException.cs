namespace ConferenceHallBooking.Domain.Exceptions;

/// <summary>
/// Thrown when attempting to delete an option that is linked to halls or used in bookings.
/// </summary>
public class OptionInUseException(Guid optionId, int hallCount, int bookingCount)
    : BusinessRuleException(
        $"Option {optionId} cannot be deleted: it is linked to {hallCount} hall(s) and used in {bookingCount} booking(s).",
        "OPTION_IN_USE")
{
    public Guid OptionId { get; } = optionId;
    public int HallCount { get; } = hallCount;
    public int BookingCount { get; } = bookingCount;
}
