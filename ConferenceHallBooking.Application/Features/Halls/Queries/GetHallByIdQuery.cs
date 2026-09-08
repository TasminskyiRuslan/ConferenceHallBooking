using ConferenceHallBooking.Application.DTOs.Halls;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Halls.Queries;

public record GetHallByIdQuery(Guid Id) : IRequest<HallResponse>;
