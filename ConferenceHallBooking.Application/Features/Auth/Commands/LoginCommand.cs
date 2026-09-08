using ConferenceHallBooking.Application.DTOs.Auth;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Auth.Commands;

public record LoginCommand(
    string Email,
    string Password) : IRequest<AuthResponse>;
