using ConferenceHallBooking.Application.DTOs;
using ConferenceHallBooking.Application.DTOs.Bookings;
using ConferenceHallBooking.Domain.Entities;

namespace ConferenceHallBooking.Application.Interfaces;

public interface IPricingService
{
    PricingResult CalculatePrice(
        decimal baseHourlyRate,
        IEnumerable<Option> selectedOptions,
        DateTimeOffset startTime,
        DateTimeOffset endTime);
}