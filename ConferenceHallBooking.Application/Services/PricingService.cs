using ConferenceHallBooking.Application.Configuration;
using ConferenceHallBooking.Application.DTOs.Bookings;
using ConferenceHallBooking.Application.Interfaces;
using ConferenceHallBooking.Domain.Entities;
using Microsoft.Extensions.Options;

namespace ConferenceHallBooking.Application.Services;

public class PricingService : IPricingService
{
    private readonly PricingSettings _settings;

    public PricingService(IOptions<PricingSettings> settings)
    {
        _settings = settings.Value;
    }

    public PricingResult CalculatePrice(
        decimal baseHourlyRate,
        IEnumerable<Option> selectedOptions,
        DateTimeOffset startTime,
        DateTimeOffset endTime)
    {
        if (endTime <= startTime)
        {
            throw new ArgumentException("The end time must be strictly greater than the start time.");
        }

        var points = GetBoundaryPoints(startTime, endTime);
        decimal hallCost = 0;

        for (int i = 0; i < points.Count - 1; i++)
        {
            DateTimeOffset segmentStart = points[i];
            DateTimeOffset segmentEnd = points[i + 1];

            double hours = (segmentEnd - segmentStart).TotalHours;

            DateTimeOffset midSegmentPoint = segmentStart.AddHours(hours / 2);
            TimeOnly midTime = TimeOnly.FromDateTime(midSegmentPoint.DateTime);

            decimal multiplier = GetMultiplierForTime(midTime);

            hallCost += baseHourlyRate * (decimal)hours * multiplier;
        }

        decimal optionsCost = selectedOptions.Sum(option => option.Price);
        decimal totalCost = hallCost + optionsCost;

        return new PricingResult(
            Math.Round(hallCost, 2),
            Math.Round(optionsCost, 2),
            Math.Round(totalCost, 2));
    }

    private List<DateTimeOffset> GetBoundaryPoints(DateTimeOffset start, DateTimeOffset end)
    {
        var points = new HashSet<DateTimeOffset> { start, end };

        var startDate = DateOnly.FromDateTime(start.Date);
        var endDate = DateOnly.FromDateTime(end.Date);

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            foreach (var rule in _settings.Rules)
            {
                var ruleStartDateTime = date.ToDateTime(rule.StartTime);
                var ruleEndDateTime = date.ToDateTime(rule.EndTime);

                var ruleStart = new DateTimeOffset(ruleStartDateTime, start.Offset);
                var ruleEnd = new DateTimeOffset(ruleEndDateTime, start.Offset);

                if (ruleStart > start && ruleStart < end)
                {
                    points.Add(ruleStart);
                }

                if (ruleEnd > start && ruleEnd < end)
                {
                    points.Add(ruleEnd);
                }
            }
        }

        return points.OrderBy(p => p).ToList();
    }

    private decimal GetMultiplierForTime(TimeOnly time)
    {
        var matchedRule = _settings.Rules
            .FirstOrDefault(r => r.StartTime <= time && time < r.EndTime);

        return matchedRule?.Multiplier ?? 1.0m;
    }
}