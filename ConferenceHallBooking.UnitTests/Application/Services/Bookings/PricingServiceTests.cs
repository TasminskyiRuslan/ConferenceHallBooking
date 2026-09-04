using ConferenceHallBooking.Application.Configuration;
using ConferenceHallBooking.Application.Exceptions;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBookingApi.ConferenceHallBooking.Application.Services.Bookings;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace ConferenceHallBooking.UnitTests.Application.Services.Bookings;

public class PricingServiceTests
{
    [Fact]
    public void CalculatePrice_WhenEndTimeIsEqualToStartTime_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var settings = Options.Create(new PricingSettings());
        var service = new PricingService(settings);
        var timePoint = DateTimeOffset.UtcNow;

        // Act
        var act = () => service.CalculatePrice(100m, null, timePoint, timePoint);

        // Assert
        act.Should().Throw<BusinessRuleException>()
            .WithMessage("The booking end time must be strictly after the start time.");
    }

    [Fact]
    public void CalculatePrice_WhenEndTimeIsBeforeStartTime_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var settings = Options.Create(new PricingSettings());
        var service = new PricingService(settings);
        var start = DateTimeOffset.UtcNow;
        var end = start.AddHours(-1);

        // Act
        var act = () => service.CalculatePrice(100m, null, start, end);

        // Assert
        act.Should().Throw<BusinessRuleException>()
            .WithMessage("The booking end time must be strictly after the start time.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void CalculatePrice_WhenBaseHourlyRateIsZeroOrNegative_ShouldThrowBusinessRuleException(decimal invalidRate)
    {
        // Arrange
        var settings = Options.Create(new PricingSettings());
        var service = new PricingService(settings);
        var start = DateTimeOffset.UtcNow;
        var end = start.AddHours(2);

        // Act
        var act = () => service.CalculatePrice(invalidRate, null, start, end);

        // Assert
        act.Should().Throw<BusinessRuleException>()
            .WithMessage("Base hourly rate must be greater than zero.");
    }

    [Fact]
    public void CalculatePrice_WithStandardRateAndNoRules_ShouldCalculateCorrectTotal()
    {
        // Arrange
        const decimal baseHourlyRate = 100m;
        const int durationHours = 3;
        const decimal expectedHallCost = baseHourlyRate * durationHours; // 300m

        var settings = Options.Create(new PricingSettings { Rules = new List<PricingRule>() });
        var service = new PricingService(settings);
        var start = new DateTimeOffset(2026, 9, 10, 10, 0, 0, TimeSpan.Zero);
        var end = start.AddHours(durationHours);

        // Act
        var result = service.CalculatePrice(baseHourlyRate, null, start, end);

        // Assert
        result.HallCost.Should().Be(expectedHallCost);
        result.OptionsCost.Should().Be(0m);
        result.TotalCost.Should().Be(expectedHallCost);
    }

    [Fact]
    public void CalculatePrice_WithSelectedOptions_ShouldIncludeOptionsCostInTotal()
    {
        // Arrange
        const decimal baseHourlyRate = 100m;
        const int durationHours = 2;
        const decimal expectedHallCost = baseHourlyRate * durationHours; // 200m

        var projectorOption = new Option("Projector", 50m);
        var wifiOption = new Option("Wi-Fi", 30m);
        var options = new List<Option> { projectorOption, wifiOption };
        var expectedOptionsCost = projectorOption.Price + wifiOption.Price; // 80m
        var expectedTotalCost = expectedHallCost + expectedOptionsCost; // 280m

        var settings = Options.Create(new PricingSettings());
        var service = new PricingService(settings);
        var start = new DateTimeOffset(2026, 9, 10, 10, 0, 0, TimeSpan.Zero);
        var end = start.AddHours(durationHours);

        // Act
        var result = service.CalculatePrice(baseHourlyRate, options, start, end);

        // Assert
        result.HallCost.Should().Be(expectedHallCost);
        result.OptionsCost.Should().Be(expectedOptionsCost);
        result.TotalCost.Should().Be(expectedTotalCost);
    }

    [Fact]
    public void CalculatePrice_WithMultipleOverlappingRules_ShouldCalculateComplexSegmentsCorrectly()
    {
        // Arrange
        var settings = Options.Create(new PricingSettings
        {
            Rules = new List<PricingRule>
            {
                new() { StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(12, 0), Multiplier = 1.2m },
                new() { StartTime = new TimeOnly(12, 0), EndTime = new TimeOnly(17, 0), Multiplier = 1.5m }
            }
        });
        var service = new PricingService(settings);

        const decimal baseHourlyRate = 100m;
        const decimal expectedHallCost = 240m + 450m;

        var start = new DateTimeOffset(2026, 9, 10, 10, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 9, 10, 15, 0, 0, TimeSpan.Zero);

        // Act
        var result = service.CalculatePrice(baseHourlyRate, null, start, end);

        // Assert
        result.HallCost.Should().Be(expectedHallCost);
        result.TotalCost.Should().Be(expectedHallCost);
    }

    [Fact]
    public void CalculatePrice_WhenBookingSpansMultipleDays_ShouldApplyRulesAcrossMidnight()
    {
        // Arrange
        var settings = Options.Create(new PricingSettings
        {
            Rules = new List<PricingRule>
            {
                new() { StartTime = new TimeOnly(21, 0), EndTime = new TimeOnly(23, 0), Multiplier = 1.5m },
                new() { StartTime = new TimeOnly(0, 0), EndTime = new TimeOnly(6, 0), Multiplier = 2.0m }
            }
        });
        var service = new PricingService(settings);

        const decimal baseHourlyRate = 100m;
        const decimal expectedHallCost = 300m + 100m + 400m;

        var start = new DateTimeOffset(2026, 9, 10, 21, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 9, 11, 2, 0, 0, TimeSpan.Zero);

        // Act
        var result = service.CalculatePrice(baseHourlyRate, null, start, end);

        // Assert
        result.HallCost.Should().Be(expectedHallCost);
        result.TotalCost.Should().Be(expectedHallCost);
    }

    [Fact]
    public void CalculatePrice_WithFractionalHoursAndMultipliers_ShouldRoundAwayFromZeroCorrectly()
    {
        // Arrange
        var settings = Options.Create(new PricingSettings
        {
            Rules = new List<PricingRule>
            {
                new() { StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(18, 0), Multiplier = 1.3333m }
            }
        });
        var service = new PricingService(settings);

        const decimal baseHourlyRate = 100m;
        const decimal expectedHallCost = 166.66m;

        var start = new DateTimeOffset(2026, 9, 10, 10, 0, 0, TimeSpan.Zero);
        var end = start.AddHours(1).AddMinutes(15);

        // Act
        var result = service.CalculatePrice(baseHourlyRate, null, start, end);

        // Assert
        result.HallCost.Should().Be(expectedHallCost);
    }

    [Fact]
    public void CalculatePrice_WhenOptionsListIsEmpty_ShouldReturnZeroOptionsCost()
    {
        // Arrange
        var settings = Options.Create(new PricingSettings());
        var service = new PricingService(settings);
        var start = new DateTimeOffset(2026, 9, 10, 10, 0, 0, TimeSpan.Zero);
        var end = start.AddHours(2);

        // Act
        var result = service.CalculatePrice(100m, new List<Option>(), start, end);

        // Assert
        result.OptionsCost.Should().Be(0m);
        result.TotalCost.Should().Be(200m);
    }
}