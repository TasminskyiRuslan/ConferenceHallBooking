using ConferenceHallBooking.Application.DTOs.Options;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Options.Queries;

public record GetOptionByIdQuery(Guid Id) : IRequest<OptionResponse>;
