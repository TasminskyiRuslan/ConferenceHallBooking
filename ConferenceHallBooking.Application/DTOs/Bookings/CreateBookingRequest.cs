namespace ConferenceHallBooking.Application.DTOs.Bookings;

public record CreateBookingRequest(
    Guid HallId,
    DateTimeOffset StartTime,
    decimal DurationHours,
    List<Guid>? OptionIds);