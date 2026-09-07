namespace ConferenceHallBooking.Domain.Exceptions;

public class InvalidBaseHourlyRateException(decimal baseHourlyRate)
    : BusinessRuleException(
        $"Base hourly rate must be greater than zero. Actual value: {baseHourlyRate}.",
        "INVALID_BASE_HOURLY_RATE")
{
    public decimal BaseHourlyRate { get; } = baseHourlyRate;
}
