using ConferenceHallBooking.Application.DTOs.Options;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Options.Commands;

public record CreateOptionCommand(
    string Name,
    decimal Price) : IRequest<OptionResponse>;
