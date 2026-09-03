
using ConferenceHallBooking.Application.DTOs.Options;

namespace ConferenceHallBooking.Application.DTOs.Halls;

public record HallResponse(
    Guid Id,
    string Name,
    int Capacity,
    decimal BaseHourlyRate,
    List<OptionResponse> Options);