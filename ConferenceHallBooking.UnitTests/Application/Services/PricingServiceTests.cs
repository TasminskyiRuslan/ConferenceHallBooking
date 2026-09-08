using ConferenceHallBooking.Application.Configuration;
using ConferenceHallBooking.Application.DTOs.Bookings;
using ConferenceHallBooking.Application.Services.Bookings;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace ConferenceHallBooking.UnitTests.Application.Services;

public class PricingServiceTests
{
    private static readonly TimeSpan Offset = TimeSpan.FromHours(2);

    private static PricingService CreateService(params PricingRule[] rules)
    {
        var settings = Options.Create(new PricingSettings { Rules = rules.ToList() });
        return new PricingService(settings);
    }

    private static DateTimeOffset Date(int day, int hour, int minute = 0) =>
        new(2024, 1, day, hour, minute, 0, Offset);

    [Fact]
    public void CalculatePrice_WhenNoRules_ShouldUseBaseRate()
    {
        var service = CreateService([]);

        var result = service.CalculatePrice(100m, null, Date(1, 10), Date(1, 13));

        result.HallCost.Should().Be(300m);
        result.OptionsCost.Should().Be(0m);
        result.TotalCost.Should().Be(300m);
    }

    [Fact]
    public void CalculatePrice_WhenNoRules_ShouldRoundHallCost()
    {
        var service = CreateService([]);

        var result = service.CalculatePrice(33.33m, null, Date(1, 8), Date(1, 10));

        result.HallCost.Should().Be(66.66m);
        result.TotalCost.Should().Be(66.66m);
    }

    [Fact]
    public void CalculatePrice_WhenRuleApplies_ShouldApplyMultiplier()
    {
        var rules = new[]
        {
            new PricingRule { StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(18, 0), Multiplier = 1.5m }
        };
        var service = CreateService(rules);

        var result = service.CalculatePrice(100m, null, Date(1, 10), Date(1, 12));

        result.HallCost.Should().Be(300m);
    }

    [Fact]
    public void CalculatePrice_WhenMultipleRulesApply_ShouldSplitIntoSegments()
    {
        var rules = new[]
        {
            new PricingRule { StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(10, 0), Multiplier = 1.0m },
            new PricingRule { StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(18, 0), Multiplier = 1.5m }
        };
        var service = CreateService(rules);

        var result = service.CalculatePrice(100m, null, Date(1, 9), Date(1, 11));

        // 09:00-10:00 (1h, middle 09:30 → rule 1.0x) = 100
        // 10:00-11:00 (1h, middle 10:30 → rule 1.5x) = 150
        result.HallCost.Should().Be(250m);
    }

    [Fact]
    public void CalculatePrice_WhenRuleGapExists_ShouldUseDefaultMultiplier()
    {
        var rules = new[]
        {
            new PricingRule { StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(14, 0), Multiplier = 2.0m }
        };
        var service = CreateService(rules);

        var result = service.CalculatePrice(100m, null, Date(1, 9), Date(1, 17));

        // 09:00-10:00 (1h, middle 09:30 → no rule, 1.0x) = 100
        // 10:00-14:00 (4h, middle 12:00 → 2.0x) = 800
        // 14:00-17:00 (3h, middle 15:30 → no rule, 1.0x) = 300
        result.HallCost.Should().Be(1200m);
    }

    [Fact]
    public void CalculatePrice_WithOptions_ShouldAddOptionsCost()
    {
        var service = CreateService([]);
        var options = new List<Option> { new("Projector", 50m) };

        var result = service.CalculatePrice(100m, options, Date(1, 10), Date(1, 13));

        result.HallCost.Should().Be(300m);
        result.OptionsCost.Should().Be(50m);
        result.TotalCost.Should().Be(350m);
    }

    [Fact]
    public void CalculatePrice_WhenEndTimeBeforeStartTime_ShouldThrowInvalidBookingTimeException()
    {
        var service = CreateService([]);

        var act = () => service.CalculatePrice(100m, null, Date(1, 13), Date(1, 10));

        act.Should().Throw<InvalidBookingTimeException>();
    }

    [Fact]
    public void CalculatePrice_WhenBaseHourlyRateIsZero_ShouldThrowInvalidBaseHourlyRateException()
    {
        var service = CreateService([]);

        var act = () => service.CalculatePrice(0m, null, Date(1, 10), Date(1, 13));

        act.Should().Throw<InvalidBaseHourlyRateException>();
    }

    [Fact]
    public void CalculatePrice_WhenBaseHourlyRateIsNegative_ShouldThrowInvalidBaseHourlyRateException()
    {
        var service = CreateService([]);

        var act = () => service.CalculatePrice(-50m, null, Date(1, 10), Date(1, 13));

        act.Should().Throw<InvalidBaseHourlyRateException>();
    }

    [Fact]
    public void CalculatePrice_WhenBookingSpansMultipleDays_ShouldHandleCorrectly()
    {
        var rules = new[]
        {
            new PricingRule { StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(18, 0), Multiplier = 1.5m }
        };
        var service = CreateService(rules);

        var result = service.CalculatePrice(100m, null, Date(1, 22), Date(3, 9));

        // Jan 1 22:00 - Jan 2 08:00 (10h, middle 03:00 → no rule, 1.0x) = 1000
        // Jan 2 08:00 - Jan 2 18:00 (10h, middle 13:00 → 1.5x) = 1500
        // Jan 2 18:00 - Jan 3 08:00 (14h, middle 01:00 → no rule, 1.0x) = 1400
        // Jan 3 08:00 - Jan 3 09:00 (1h, middle 08:30 → 1.5x) = 150
        result.HallCost.Should().Be(4050m);
    }
}
