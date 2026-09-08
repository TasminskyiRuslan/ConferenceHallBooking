using ConferenceHallBooking.Application.DTOs.Auth;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Auth.Commands;

public record RegisterCommand(
    string Email,
    string Password,
    string ConfirmPassword,
    string FullName) : IRequest<AuthResponse>;
