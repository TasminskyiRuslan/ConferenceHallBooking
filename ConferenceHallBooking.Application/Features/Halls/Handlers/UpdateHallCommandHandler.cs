using ConferenceHallBooking.Application.DTOs.Halls;
using ConferenceHallBooking.Application.Features.Halls.Commands;
using ConferenceHallBooking.Application.Interfaces.Halls;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Halls.Handlers;

public class UpdateHallCommandHandler(IHallService hallService)
    : IRequestHandler<UpdateHallCommand>
{
    public async Task Handle(UpdateHallCommand request, CancellationToken cancellationToken)
    {
        var updateRequest = new UpdateHallRequest(
            request.Name,
            request.Capacity,
            request.BaseHourlyRate,
            request.OptionIds);

        await hallService.UpdateAsync(request.Id, updateRequest, cancellationToken);
    }
}
