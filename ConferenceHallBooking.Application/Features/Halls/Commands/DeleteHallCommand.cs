using MediatR;

namespace ConferenceHallBooking.Application.Features.Halls.Commands;

public record DeleteHallCommand(Guid Id) : IRequest;
