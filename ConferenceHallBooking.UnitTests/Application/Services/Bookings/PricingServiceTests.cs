using ConferenceHallBooking.Application.Configuration;
using ConferenceHallBooking.Application.Services.Bookings;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace ConferenceHallBooking.UnitTests.Application.Services.Bookings;

public class PricingServiceTests
{
    private static readonly DateTimeOffset Reference = new(2026, 9, 10, 10, 0, 0, TimeSpan.Zero);

    #region Validation

    [Fact]
    public void CalculatePrice_WhenEndTimeEqualsStartTime_ShouldThrow()
    {
        var service = CreatePricingService();

        var act = () => service.CalculatePrice(100m, null, Reference, Reference);

        act.Should().Throw<InvalidBookingTimeException>();
    }

    [Fact]
    public void CalculatePrice_WhenEndTimeBeforeStartTime_ShouldThrow()
    {
        var service = CreatePricingService();

        var act = () => service.CalculatePrice(100m, null, Reference, Reference.AddHours(-1));

        act.Should().Throw<InvalidBookingTimeException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    [InlineData(-0.01)]
    public void CalculatePrice_WhenBaseHourlyRateIsZeroOrNegative_ShouldThrow(decimal invalidRate)
    {
        var service = CreatePricingService();

        var act = () => service.CalculatePrice(invalidRate, null, Reference, Reference.AddHours(2));

        act.Should().Throw<InvalidBaseHourlyRateException>();
    }

    #endregion

    #region Standard Calculations

    [Fact]
    public void CalculatePrice_WithNoRules_ShouldCalculateStandardRate()
    {
        var service = CreatePricingService(new PricingSettings { Rules = [] });

        var result = service.CalculatePrice(100m, null, Reference, Reference.AddHours(3));

        result.HallCost.Should().Be(300m);
        result.OptionsCost.Should().Be(0m);
        result.TotalCost.Should().Be(300m);
    }

    [Fact]
    public void CalculatePrice_WithOptions_ShouldAddOptionsCostToTotal()
    {
        var service = CreatePricingService(new PricingSettings { Rules = [] });

        var projector = new Option("Projector", 50m);
        var wifi = new Option("Wi-Fi", 30m);

        var result = service.CalculatePrice(100m, [projector, wifi], Reference, Reference.AddHours(2));

        result.HallCost.Should().Be(200m);
        result.OptionsCost.Should().Be(80m);
        result.TotalCost.Should().Be(280m);
    }

    [Fact]
    public void CalculatePrice_WithNullOptions_ShouldReturnZeroOptionsCost()
    {
        var service = CreatePricingService(new PricingSettings { Rules = [] });

        var result = service.CalculatePrice(100m, null, Reference, Reference.AddHours(2));

        result.OptionsCost.Should().Be(0m);
        result.TotalCost.Should().Be(200m);
    }

    #endregion

    #region Rounding

    [Fact]
    public void CalculatePrice_WithFractionalHours_ShouldRoundHallCost()
    {
        var service = CreatePricingService(new PricingSettings { Rules = [] });

        var result = service.CalculatePrice(100m, null, Reference, Reference.AddHours(1).AddMinutes(15));

        result.HallCost.Should().Be(125m);
    }

    [Fact]
    public void CalculatePrice_WithFractionalMultiplier_ShouldRoundAwayFromZero()
    {
        var service = CreatePricingService(new PricingSettings
        {
            Rules =
            [
                new() { StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(18, 0), Multiplier = 1.3333m }
            ]
        });

        var start = new DateTimeOffset(2026, 9, 10, 10, 0, 0, TimeSpan.Zero);
        var end = start.AddHours(1).AddMinutes(15);

        var result = service.CalculatePrice(100m, null, start, end);

        result.HallCost.Should().Be(166.66m);
    }

    [Fact]
    public void CalculatePrice_WithFractionalOptionsCost_ShouldRoundOptionsCost()
    {
        var service = CreatePricingService(new PricingSettings { Rules = [] });

        var option1 = new Option("A", 33.333m);
        var option2 = new Option("B", 33.333m);

        var result = service.CalculatePrice(100m, [option1, option2], Reference, Reference.AddHours(1));

        result.OptionsCost.Should().Be(66.67m);
    }

    #endregion

    #region Pricing Rules

    [Fact]
    public void CalculatePrice_WhenTimeFallsOutsideAllRules_ShouldUseDefaultMultiplier()
    {
        var service = CreatePricingService(new PricingSettings
        {
            Rules =
            [
                new() { StartTime = new TimeOnly(2, 0), EndTime = new TimeOnly(5, 0), Multiplier = 2.0m }
            ]
        });

        var start = new DateTimeOffset(2026, 9, 10, 10, 0, 0, TimeSpan.Zero);
        var end = start.AddHours(3);

        var result = service.CalculatePrice(100m, null, start, end);

        result.HallCost.Should().Be(300m);
    }

    [Fact]
    public void CalculatePrice_CrossingMultipleRuleBoundaries_ShouldSplitIntoSegments()
    {
        var service = CreatePricingService(new PricingSettings
        {
            Rules =
            [
                new() { StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(12, 0), Multiplier = 1.2m },
                new() { StartTime = new TimeOnly(12, 0), EndTime = new TimeOnly(17, 0), Multiplier = 1.5m }
            ]
        });

        var start = new DateTimeOffset(2026, 9, 10, 10, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 9, 10, 15, 0, 0, TimeSpan.Zero);

        var result = service.CalculatePrice(100m, null, start, end);

        result.HallCost.Should().Be(240m + 450m);
    }

    [Fact]
    public void CalculatePrice_SpanningMultipleDays_ShouldApplyRulesForEachDay()
    {
        var service = CreatePricingService(new PricingSettings
        {
            Rules =
            [
                new() { StartTime = new TimeOnly(21, 0), EndTime = new TimeOnly(23, 0), Multiplier = 1.5m },
                new() { StartTime = new TimeOnly(0, 0), EndTime = new TimeOnly(6, 0), Multiplier = 2.0m }
            ]
        });

        var start = new DateTimeOffset(2026, 9, 10, 21, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 9, 11, 2, 0, 0, TimeSpan.Zero);

        var result = service.CalculatePrice(100m, null, start, end);

        result.HallCost.Should().Be(300m + 100m + 400m);
    }

    [Fact]
    public void CalculatePrice_WhenBookingFallsBetweenRuleGaps_ShouldUseDefaultForGapHours()
    {
        var service = CreatePricingService(new PricingSettings
        {
            Rules =
            [
                new() { StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(12, 0), Multiplier = 1.5m }
            ]
        });

        var start = new DateTimeOffset(2026, 9, 10, 10, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 9, 10, 15, 0, 0, TimeSpan.Zero);

        var result = service.CalculatePrice(100m, null, start, end);

        result.HallCost.Should().Be(300m + 300m);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void CalculatePrice_WithFractionalDurationAndRules_ShouldCalculateSegmentsCorrectly()
    {
        var service = CreatePricingService(new PricingSettings
        {
            Rules =
            [
                new() { StartTime = new TimeOnly(12, 0), EndTime = new TimeOnly(14, 0), Multiplier = 2.0m }
            ]
        });

        var start = new DateTimeOffset(2026, 9, 10, 11, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 9, 10, 13, 30, 0, TimeSpan.Zero);

        var result = service.CalculatePrice(100m, null, start, end);

        result.HallCost.Should().Be(400m);
    }

    [Fact]
    public void CalculatePrice_TotalCost_ShouldBeSumOfHallAndOptionsCosts()
    {
        var service = CreatePricingService(new PricingSettings { Rules = [] });

        var option = new Option("Sound", 150m);

        var result = service.CalculatePrice(200m, [option], Reference, Reference.AddHours(3));

        result.TotalCost.Should().Be(result.HallCost + result.OptionsCost);
    }

    #endregion

    private static PricingService CreatePricingService(PricingSettings? settings = null)
    {
        return new PricingService(Options.Create(settings ?? new PricingSettings()));
    }
}
