using MediatR;

namespace ConferenceHallBooking.Application.Features.Halls.Commands;

public record UpdateHallCommand(
    Guid Id,
    string Name,
    int Capacity,
    decimal BaseHourlyRate,
    List<Guid>? OptionIds) : IRequest;
