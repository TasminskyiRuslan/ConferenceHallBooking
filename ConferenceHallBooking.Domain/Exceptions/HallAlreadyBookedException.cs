namespace ConferenceHallBooking.Domain.Exceptions;

public class HallAlreadyBookedException(Guid hallId, DateTimeOffset startTime, DateTimeOffset endTime)
    : BusinessRuleException(
        $"Conference hall with ID '{hallId}' is already booked for the time slot between {startTime:O} and {endTime:O}.",
        "HALL_ALREADY_BOOKED")
{
    public Guid HallId { get; } = hallId;
    public DateTimeOffset StartTime { get; } = startTime;
    public DateTimeOffset EndTime { get; } = endTime;
}
