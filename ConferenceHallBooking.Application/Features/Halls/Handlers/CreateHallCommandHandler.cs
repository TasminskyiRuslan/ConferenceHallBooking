using ConferenceHallBooking.Application.DTOs.Halls;
using ConferenceHallBooking.Application.Extensions;
using ConferenceHallBooking.Application.Features.Halls.Commands;
using ConferenceHallBooking.Application.Mappers;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Halls.Handlers;

public class CreateHallCommandHandler(
    IHallRepository hallRepository,
    IOptionRepository optionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateHallCommand, HallResponse>
{
    public async Task<HallResponse> Handle(CreateHallCommand request, CancellationToken cancellationToken)
    {
        var optionIds = new HashSet<Guid>(request.OptionIds ?? []);

        await optionRepository.GetByIdsOrThrowAsync(optionIds, cancellationToken);

        var hall = new Hall(request.Name, request.Capacity, request.BaseHourlyRate);

        foreach (var optionId in optionIds)
        {
            hall.AddOption(optionId);
        }

        await hallRepository.AddAsync(hall, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await hallRepository.GetByIdAsync(hall.Id, cancellationToken)
            ?? throw new NotFoundException<Hall>(hall.Id.ToString());

        return HallMapper.MapToResponse(created);
    }
}
