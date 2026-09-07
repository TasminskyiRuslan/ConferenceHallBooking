namespace ConferenceHallBooking.Application.Configuration;

public sealed record PricingSettings
{
    public const string SectionName = "PricingSettings";

    public List<PricingRule> Rules { get; init; } = [];
}
