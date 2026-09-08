using ConferenceHallBooking.Application.DTOs.Halls;
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

        await SynchronizeOptionsAsync(hall, request.Options, cancellationToken);

        hallRepository.Update(hall);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await hallRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException<Hall>(request.Id.ToString());

        return HallMapper.MapToResponse(updated);
    }

    private async Task SynchronizeOptionsAsync(Hall hall, List<DTOs.Options.InlineOption>? targetOptions, CancellationToken cancellationToken)
    {
        var desiredIds = new HashSet<Guid>();

        if (targetOptions is { Count: > 0 })
        {
            foreach (var inline in targetOptions)
            {
                var existing = await optionRepository.GetByNameAsync(inline.Name, cancellationToken);

                if (existing is not null)
                {
                    desiredIds.Add(existing.Id);
                }
                else
                {
                    var option = new Option(inline.Name, inline.Price);
                    await optionRepository.AddAsync(option, cancellationToken);
                    desiredIds.Add(option.Id);
                }
            }
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
