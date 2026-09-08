using ConferenceHallBooking.Application.DTOs.Options;
using ConferenceHallBooking.Application.Features.Options.Commands;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Options.Handlers;

public class UpdateOptionCommandHandler(
    IOptionRepository optionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateOptionCommand, OptionResponse>
{
    public async Task<OptionResponse> Handle(UpdateOptionCommand request, CancellationToken cancellationToken)
    {
        var option = await optionRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException<Option>(request.Id.ToString());

        option.Update(request.Name, request.Price);

        optionRepository.Update(option);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new OptionResponse(option.Id, option.Name, option.Price);
    }
}
