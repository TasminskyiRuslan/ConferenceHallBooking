using ConferenceHallBooking.Application.DTOs.Halls;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Halls.Commands;

public record CreateHallCommand(
    string Name,
    int Capacity,
    decimal BaseHourlyRate,
    List<Guid>? OptionIds) : IRequest<HallResponse>;
