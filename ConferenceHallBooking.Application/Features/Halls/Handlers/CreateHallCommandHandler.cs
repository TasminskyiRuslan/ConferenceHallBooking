using ConferenceHallBooking.Application.DTOs.Halls;
using ConferenceHallBooking.Application.Features.Halls.Commands;
using ConferenceHallBooking.Application.Interfaces.Halls;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Halls.Handlers;

public class CreateHallCommandHandler(IHallService hallService)
    : IRequestHandler<CreateHallCommand, Guid>
{
    public async Task<Guid> Handle(CreateHallCommand request, CancellationToken cancellationToken)
    {
        var createRequest = new CreateHallRequest(
            request.Name,
            request.Capacity,
            request.BaseHourlyRate,
            request.OptionIds);

        return await hallService.CreateAsync(createRequest, cancellationToken);
    }
}
