using ConferenceHallBooking.Application.Features.Options.Commands;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Options.Handlers;

public class DeleteOptionCommandHandler(
    IOptionRepository optionRepository,
    IHallRepository hallRepository,
    IBookingRepository bookingRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteOptionCommand>
{
    public async Task Handle(DeleteOptionCommand request, CancellationToken cancellationToken)
    {
        var option = await optionRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException<Option>(request.Id.ToString());

        var hallCount = await hallRepository.GetHallCountByOptionIdAsync(request.Id, cancellationToken);
        var bookingCount = await bookingRepository.GetBookingCountByOptionIdAsync(request.Id, cancellationToken);

        if (hallCount > 0 || bookingCount > 0)
        {
            throw new OptionInUseException(request.Id, hallCount, bookingCount);
        }

        optionRepository.Delete(option);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
