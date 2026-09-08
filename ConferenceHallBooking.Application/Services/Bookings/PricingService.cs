using ConferenceHallBooking.Application.Configuration;
using ConferenceHallBooking.Application.DTOs.Bookings;
using ConferenceHallBooking.Application.Interfaces.Bookings;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using Microsoft.Extensions.Options;

namespace ConferenceHallBooking.Application.Services.Bookings;

/// <summary>
/// Calculates booking costs using time-based pricing rules.
/// 
/// The algorithm works by splitting the booking time into segments at rule boundaries,
/// then applying the appropriate multiplier to each segment. For example, a booking
/// from 10:00 to 16:00 with a peak rule at 12:00-14:00 becomes:
///   10:00-12:00 (standard) + 12:00-14:00 (peak +15%) + 14:00-16:00 (standard).
/// 
/// Rules are defined in appsettings.json under PricingSettings.Rules.
/// If no rule matches a time slot, the base rate (multiplier 1.0) is used.
/// </summary>
public class PricingService(IOptions<PricingSettings> settings) : IPricingService
{
    /// <summary>
    /// Calculates the total cost of a booking based on time-of-day pricing rules.
    /// </summary>
    /// <param name="baseHourlyRate">Hall's base hourly rate in UAH.</param>
    /// <param name="selectedOptions">Services chosen (each adds its fixed price).</param>
    /// <param name="startTime">Booking start.</param>
    /// <param name="endTime">Booking end.</param>
    /// <returns>Breakdown of hall cost, options cost, and total cost.</returns>
    /// <exception cref="InvalidBookingTimeException">Thrown when endTime &lt;= startTime.</exception>
    /// <exception cref="InvalidBaseHourlyRateException">Thrown when rate &lt;= 0.</exception>
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

    /// <summary>
    /// Sums the cost of each time segment using the appropriate multiplier.
    /// </summary>
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

    /// <summary>
    /// Determines the multiplier for a time segment by evaluating its midpoint against the rules.
    /// </summary>
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

    /// <summary>
    /// Generates boundary points where pricing rules change.
    /// For each day in the booking range, inserts rule start/end times that fall within the range.
    /// This allows the algorithm to split bookings across multiple pricing periods.
    /// </summary>
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

    /// <summary>Adds a boundary point if it falls strictly within the booking range.</summary>
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

    /// <summary>
    /// Returns the multiplier for a specific time of day.
    /// Uses FirstOrDefault — first matching rule wins. Default is 1.0 (base rate).
    /// Range semantics: [StartTime, EndTime) — inclusive start, exclusive end.
    /// </summary>
    private static decimal GetMultiplierForTime(TimeOnly time, IReadOnlyList<PricingRule> rules)
    {
        var rule = rules
            .FirstOrDefault(r => time >= r.StartTime && time < r.EndTime);

        return rule?.Multiplier ?? 1.0m;
    }
}
