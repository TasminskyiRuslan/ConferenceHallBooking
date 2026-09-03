namespace ConferenceHallBooking.Application.DTOs.Halls;

public record CreateHallRequest(
    string Name,
    int Capacity,
    decimal BaseHourlyRate,
    List<Guid> OptionIds);