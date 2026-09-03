namespace ConferenceHallBooking.Application.Configuration;

public class PricingRule
{
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public decimal Multiplier { get; set; }
}