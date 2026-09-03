namespace ConferenceHallBooking.Application.DTOs.Halls;

public record SearchAvailableHallsRequest(
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    int Capacity);