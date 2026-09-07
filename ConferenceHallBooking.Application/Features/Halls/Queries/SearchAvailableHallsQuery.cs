using ConferenceHallBooking.Application.DTOs.Halls;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Halls.Queries;

public record SearchAvailableHallsQuery(
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    int Capacity) : IRequest<IReadOnlyCollection<HallResponse>>;
