namespace ConferenceHallBooking.Application.Configuration;

public class PricingSettings
{
    public const string SectionName = "PricingSettings";

    public List<PricingRule> Rules { get; set; } = new();
}