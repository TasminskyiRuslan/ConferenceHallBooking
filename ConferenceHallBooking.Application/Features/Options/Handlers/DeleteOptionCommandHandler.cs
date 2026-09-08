using ConferenceHallBooking.Application.Features.Options.Commands;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Options.Handlers;

public class DeleteOptionCommandHandler(
    IOptionRepository optionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteOptionCommand>
{
    public async Task Handle(DeleteOptionCommand request, CancellationToken cancellationToken)
    {
        var option = await optionRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException<Option>(request.Id.ToString());

        optionRepository.Delete(option);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
