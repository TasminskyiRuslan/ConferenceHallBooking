using ConferenceHallBooking.Application.DTOs.Bookings;
using ConferenceHallBooking.Domain.Entities;

namespace ConferenceHallBooking.Application.Interfaces.Bookings;

/// <summary>
/// Calculates booking costs using time-based pricing rules.
/// </summary>
public interface IPricingService
{
    PricingResult CalculatePrice(
        decimal baseHourlyRate,
        IEnumerable<Option>? selectedOptions,
        DateTimeOffset startTime,
        DateTimeOffset endTime);
}
