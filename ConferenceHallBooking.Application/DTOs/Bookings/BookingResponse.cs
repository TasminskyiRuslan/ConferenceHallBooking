using ConferenceHallBooking.Application.DTOs.Options;

namespace ConferenceHallBooking.Application.DTOs.Bookings;

public record BookingResponse(
    Guid Id,
    Guid HallId,
    string HallName,
    int HallCapacity,
    decimal HallBaseHourlyRate,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    int DurationHours,
    List<OptionResponse> SelectedOptions,
    decimal TotalCost);