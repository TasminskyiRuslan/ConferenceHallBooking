namespace ConferenceHallBooking.Domain.Exceptions;

public class InvalidBookingTimeException(DateTimeOffset startTime, DateTimeOffset endTime)
    : BusinessRuleException(
        $"The booking end time must be strictly after the start time. Start: {startTime:O}, End: {endTime:O}.",
        "INVALID_BOOKING_TIME")
{
    public DateTimeOffset StartTime { get; } = startTime;
    public DateTimeOffset EndTime { get; } = endTime;
}
