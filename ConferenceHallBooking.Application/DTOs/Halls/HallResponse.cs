namespace ConferenceHallBooking.Application.DTOs.Halls;

using ConferenceHallBooking.Application.DTOs.Options;

public record HallResponse(
    Guid Id,
    string Name,
    int Capacity,
    decimal BaseHourlyRate,
    IReadOnlyCollection<OptionResponse> Options);