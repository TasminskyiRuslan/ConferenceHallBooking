namespace ConferenceHallBooking.Application.DTOs.Halls;

public record UpdateHallRequest(
    string Name,
    int Capacity,
    decimal BaseHourlyRate,
    List<Guid> OptionIds);