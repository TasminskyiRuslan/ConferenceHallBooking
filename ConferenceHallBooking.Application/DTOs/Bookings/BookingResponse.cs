using ConferenceHallBooking.Application.DTOs.Options;

namespace ConferenceHallBooking.Application.DTOs.Bookings;

/// <summary>
/// Response model for a conference hall booking.
/// </summary>
public record BookingResponse(
    Guid Id,
    Guid HallId,
    string HallName,
    int HallCapacity,
    decimal HallBaseHourlyRate,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    decimal DurationHours,
    IReadOnlyCollection<OptionResponse> SelectedOptions,
    decimal HallCost,
    decimal OptionsCost,
    decimal TotalCost);
