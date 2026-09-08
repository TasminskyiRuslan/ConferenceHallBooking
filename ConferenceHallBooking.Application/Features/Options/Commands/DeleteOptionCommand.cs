using MediatR;

namespace ConferenceHallBooking.Application.Features.Options.Commands;

public record DeleteOptionCommand(Guid Id) : IRequest;
