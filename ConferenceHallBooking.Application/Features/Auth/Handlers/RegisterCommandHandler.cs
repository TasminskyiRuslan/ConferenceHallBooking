using ConferenceHallBooking.Application.DTOs.Auth;
using ConferenceHallBooking.Application.Features.Auth.Commands;
using ConferenceHallBooking.Application.Interfaces.Auth;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Auth.Handlers;

public class RegisterCommandHandler(IAuthService authService)
    : IRequestHandler<RegisterCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        return await authService.RegisterAsync(request, cancellationToken);
    }
}
