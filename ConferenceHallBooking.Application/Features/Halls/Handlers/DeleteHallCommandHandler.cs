using ConferenceHallBooking.Application.Features.Halls.Commands;
using ConferenceHallBooking.Application.Interfaces.Halls;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Halls.Handlers;

public class DeleteHallCommandHandler(IHallService hallService)
    : IRequestHandler<DeleteHallCommand>
{
    public async Task Handle(DeleteHallCommand request, CancellationToken cancellationToken)
    {
        await hallService.DeleteAsync(request.Id, cancellationToken);
    }
}
