namespace ConferenceHallBooking.Application.Configuration;

public sealed record PricingRule
{
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public decimal Multiplier { get; init; }
}
