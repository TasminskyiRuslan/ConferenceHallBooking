using ConferenceHallBooking.Application.Configuration;
using ConferenceHallBooking.Application.DTOs.Bookings;
using ConferenceHallBooking.Application.Interfaces.Bookings;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using Microsoft.Extensions.Options;

namespace ConferenceHallBooking.Application.Services.Bookings;

public class PricingService(IOptions<PricingSettings> settings) : IPricingService
{
    public PricingResult CalculatePrice(
        decimal baseHourlyRate,
        IEnumerable<Option>? selectedOptions,
        DateTimeOffset startTime,
        DateTimeOffset endTime)
    {
        if (endTime <= startTime)
        {
            throw new InvalidBookingTimeException(startTime, endTime);
        }

        if (baseHourlyRate <= 0)
        {
            throw new InvalidBaseHourlyRateException(baseHourlyRate);
        }

        var rules = settings.Value.Rules;
        var boundaryPoints = GetBoundaryPoints(startTime, endTime, rules);
        var hallCost = CalculateHallCost(baseHourlyRate, boundaryPoints, startTime, rules);
        var optionsCost = selectedOptions?.Sum(option => option.Price) ?? 0m;

        var roundedHallCost = Math.Round(hallCost, 2, MidpointRounding.AwayFromZero);
        var roundedOptionsCost = Math.Round(optionsCost, 2, MidpointRounding.AwayFromZero);
        var roundedTotalCost = roundedHallCost + roundedOptionsCost;

        return new PricingResult(roundedHallCost, roundedOptionsCost, roundedTotalCost);
    }

    private static decimal CalculateHallCost(
        decimal baseHourlyRate,
        IReadOnlyList<DateTimeOffset> boundaryPoints,
        DateTimeOffset referenceOffset,
        IReadOnlyList<PricingRule> rules)
    {
        decimal hallCost = 0m;

        for (var i = 0; i < boundaryPoints.Count - 1; i++)
        {
            var segmentStart = boundaryPoints[i];
            var segmentEnd = boundaryPoints[i + 1];

            var hours = (decimal)(segmentEnd - segmentStart).Ticks / TimeSpan.TicksPerHour;
            var multiplier = GetMultiplierForSegment(segmentStart, segmentEnd, referenceOffset, rules);

            hallCost += baseHourlyRate * hours * multiplier;
        }

        return hallCost;
    }

    private static decimal GetMultiplierForSegment(
        DateTimeOffset segmentStart,
        DateTimeOffset segmentEnd,
        DateTimeOffset referenceOffset,
        IReadOnlyList<PricingRule> rules)
    {
        var middleTicks = segmentStart.Ticks + (segmentEnd.Ticks - segmentStart.Ticks) / 2;
        var middlePoint = new DateTimeOffset(middleTicks, referenceOffset.Offset);
        var time = TimeOnly.FromDateTime(middlePoint.DateTime);

        return GetMultiplierForTime(time, rules);
    }

    private static List<DateTimeOffset> GetBoundaryPoints(
        DateTimeOffset start,
        DateTimeOffset end,
        IReadOnlyList<PricingRule> rules)
    {
        var points = new HashSet<DateTimeOffset> { start, end };

        if (rules.Count == 0)
        {
            return [.. points.OrderBy(point => point)];
        }

        var startDate = DateOnly.FromDateTime(start.Date);
        var endDate = DateOnly.FromDateTime(end.Date);

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            foreach (var rule in rules)
            {
                AddBoundaryIfInRange(points, start, end, date, rule.StartTime);
                AddBoundaryIfInRange(points, start, end, date, rule.EndTime);
            }
        }

        return [.. points.OrderBy(point => point)];
    }

    private static void AddBoundaryIfInRange(
        HashSet<DateTimeOffset> points,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        DateOnly date,
        TimeOnly time)
    {
        var point = new DateTimeOffset(date.ToDateTime(time), rangeStart.Offset);

        if (point > rangeStart && point < rangeEnd)
        {
            points.Add(point);
        }
    }

    private static decimal GetMultiplierForTime(TimeOnly time, IReadOnlyList<PricingRule> rules)
    {
        var rule = rules
            .FirstOrDefault(r => time >= r.StartTime && time < r.EndTime);

        return rule?.Multiplier ?? 1.0m;
    }
}
