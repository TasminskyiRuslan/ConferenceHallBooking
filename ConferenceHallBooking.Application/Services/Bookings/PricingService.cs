using ConferenceHallBooking.Application.Configuration;
using ConferenceHallBooking.Application.DTOs.Bookings;
using ConferenceHallBooking.Application.Exceptions;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBookingApi.ConferenceHallBooking.Application.Interfaces.Bookings;
using Microsoft.Extensions.Options;

namespace ConferenceHallBookingApi.ConferenceHallBooking.Application.Services.Bookings;

public class PricingService : IPricingService
{
    private readonly PricingSettings _settings;

    public PricingService(IOptions<PricingSettings> settings)
    {
        _settings = settings.Value;
    }

    public PricingResult CalculatePrice(
        decimal baseHourlyRate,
        IEnumerable<Option>? selectedOptions,
        DateTimeOffset startTime,
        DateTimeOffset endTime)
    {
        if (endTime <= startTime)
        {
            throw new BusinessRuleException(
                "The booking end time must be strictly after the start time.");
        }

        if (baseHourlyRate <= 0)
        {
            throw new BusinessRuleException(
                "Base hourly rate must be greater than zero.");
        }

        var boundaryPoints = GetBoundaryPoints(startTime, endTime);

        decimal hallCost = 0m;

        for (var i = 0; i < boundaryPoints.Count - 1; i++)
        {
            var segmentStart = boundaryPoints[i];
            var segmentEnd = boundaryPoints[i + 1];

            var hours = (decimal)(segmentEnd - segmentStart).Ticks / TimeSpan.TicksPerHour;

            var middleTicks = segmentStart.Ticks + (segmentEnd.Ticks - segmentStart.Ticks) / 2;
            var middlePoint = new DateTimeOffset(middleTicks, startTime.Offset);
            var time = TimeOnly.FromDateTime(middlePoint.DateTime);

            var multiplier = GetMultiplierForTime(time);

            hallCost += baseHourlyRate * hours * multiplier;
        }

        var optionsCost = selectedOptions?.Sum(option => option.Price) ?? 0m;
        var totalCost = hallCost + optionsCost;

        return new PricingResult(
            Math.Round(hallCost, 2, MidpointRounding.AwayFromZero),
            Math.Round(optionsCost, 2, MidpointRounding.AwayFromZero),
            Math.Round(totalCost, 2, MidpointRounding.AwayFromZero));
    }

    private List<DateTimeOffset> GetBoundaryPoints(
        DateTimeOffset start,
        DateTimeOffset end)
    {
        var points = new HashSet<DateTimeOffset> { start, end };

        if (_settings.Rules.Count == 0)
        {
            return points.OrderBy(point => point).ToList();
        }

        var startDate = DateOnly.FromDateTime(start.Date);
        var endDate = DateOnly.FromDateTime(end.Date);

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            foreach (var rule in _settings.Rules)
            {
                var ruleStart = new DateTimeOffset(date.ToDateTime(rule.StartTime), start.Offset);
                var ruleEnd = new DateTimeOffset(date.ToDateTime(rule.EndTime), start.Offset);

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

        return points.OrderBy(point => point).ToList();
    }

    private decimal GetMultiplierForTime(TimeOnly time)
    {
        var rule = _settings.Rules
            .FirstOrDefault(r => time >= r.StartTime && time < r.EndTime);

        return rule?.Multiplier ?? 1.0m;
    }
}