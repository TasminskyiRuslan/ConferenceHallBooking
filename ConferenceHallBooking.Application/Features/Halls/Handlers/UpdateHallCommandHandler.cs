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
    IBookingRepository bookingRepository,
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

        var optionIdsToRemove = currentIds.Where(id => !desiredIds.Contains(id)).ToList();

        if (optionIdsToRemove.Count > 0)
        {
            var usedOptionIds = await bookingRepository.GetUsedOptionIdsByHallAsync(hall.Id, cancellationToken);
            var usedIdsToRemove = optionIdsToRemove.Where(id => usedOptionIds.Contains(id)).ToList();

            if (usedIdsToRemove.Count > 0)
            {
                throw new HallOptionInUseException(hall.Id, usedIdsToRemove);
            }

            foreach (var id in optionIdsToRemove)
            {
                hall.RemoveOption(id);
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
