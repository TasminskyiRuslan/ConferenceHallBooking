namespace ConferenceHallBooking.Application.DTOs.Bookings;

public record CreateBookingRequest(
    Guid HallId,
    DateTimeOffset StartTime,
    int DurationHours,
    List<Guid>? OptionIds);