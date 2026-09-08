using ConferenceHallBooking.Application.DTOs.Options;
using ConferenceHallBooking.Application.Features.Options.Commands;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Options.Handlers;

public class CreateOptionCommandHandler(
    IOptionRepository optionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateOptionCommand, OptionResponse>
{
    public async Task<OptionResponse> Handle(CreateOptionCommand request, CancellationToken cancellationToken)
    {
        var existing = await optionRepository.GetByNameAsync(request.Name, cancellationToken);

        if (existing is not null)
        {
            throw new OptionNameAlreadyExistsException(request.Name);
        }

        var option = new Option(request.Name, request.Price);

        await optionRepository.AddAsync(option, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new OptionResponse(option.Id, option.Name, option.Price);
    }
}
