namespace ConferenceHallBooking.Domain.Exceptions;

public class InvalidBookingDurationException(decimal durationHours)
    : BusinessRuleException(
        $"Booking duration must be greater than zero. Actual value: {durationHours} hours.",
        "INVALID_BOOKING_DURATION")
{
    public decimal DurationHours { get; } = durationHours;
}
