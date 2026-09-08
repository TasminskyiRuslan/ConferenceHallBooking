using ConferenceHallBooking.Application.DTOs.Options;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Options.Queries;

public record GetAllOptionsQuery : IRequest<IReadOnlyCollection<OptionResponse>>;
