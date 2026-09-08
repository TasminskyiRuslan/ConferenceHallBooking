using ConferenceHallBooking.Application.DTOs.Halls;
using ConferenceHallBooking.Application.DTOs.Options;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Halls.Commands;

public record UpdateHallCommand(
    Guid Id,
    string Name,
    int Capacity,
    decimal BaseHourlyRate,
    List<InlineOption>? Options) : IRequest<HallResponse>;
