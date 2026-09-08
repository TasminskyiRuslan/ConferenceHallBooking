using ConferenceHallBooking.Application.DTOs.Halls;
using ConferenceHallBooking.Application.Extensions;
using ConferenceHallBooking.Application.Features.Halls.Commands;
using ConferenceHallBooking.Application.Mappers;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Halls.Handlers;

public class UpdateHallCommandHandler(
    IHallRepository hallRepository,
    IOptionRepository optionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateHallCommand, HallResponse>
{
    public async Task<HallResponse> Handle(UpdateHallCommand request, CancellationToken cancellationToken)
    {
        var hall = await hallRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException<Hall>(request.Id.ToString());

        if (hall.Name != request.Name && await hallRepository.ExistsByNameAsync(request.Name, cancellationToken))
        {
            throw new HallNameAlreadyExistsException(request.Name);
        }

        hall.Update(request.Name, request.Capacity, request.BaseHourlyRate);

        await SynchronizeOptionsAsync(hall, request.OptionIds, cancellationToken);

        hallRepository.Update(hall);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await hallRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException<Hall>(request.Id.ToString());

        return HallMapper.MapToResponse(updated);
    }

    private async Task SynchronizeOptionsAsync(Hall hall, List<Guid>? targetOptionIds, CancellationToken cancellationToken)
    {
        HashSet<Guid> desiredIds;

        if (targetOptionIds is { Count: > 0 })
        {
            await optionRepository.GetByIdsOrThrowAsync(targetOptionIds, cancellationToken);
            desiredIds = new HashSet<Guid>(targetOptionIds);
        }
        else
        {
            desiredIds = [];
        }

        var currentIds = new HashSet<Guid>(hall.HallOptions.Select(ho => ho.OptionId));

        foreach (var currentId in currentIds)
        {
            if (!desiredIds.Contains(currentId))
            {
                hall.RemoveOption(currentId);
            }
        }

        foreach (var targetId in desiredIds)
        {
            if (!currentIds.Contains(targetId))
            {
                hall.AddOption(targetId);
            }
        }
    }
}
