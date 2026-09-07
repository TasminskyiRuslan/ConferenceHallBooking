using ConferenceHallBooking.Application.DTOs.Halls;
using ConferenceHallBooking.Application.DTOs.Options;
using ConferenceHallBooking.Application.Extensions;
using ConferenceHallBooking.Application.Interfaces.Halls;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;

namespace ConferenceHallBooking.Application.Services.Halls;

public class HallService(
    IHallRepository hallRepository,
    IOptionRepository optionRepository,
    IUnitOfWork unitOfWork) : IHallService
{
    public async Task<Guid> CreateAsync(CreateHallRequest request, CancellationToken cancellationToken = default)
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

        return hall.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateHallRequest request, CancellationToken cancellationToken = default)
    {
        var hall = await hallRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException<Hall>(id.ToString());

        hall.Update(request.Name, request.Capacity, request.BaseHourlyRate);

        await SynchronizeOptionsAsync(hall, request.OptionIds, cancellationToken);

        hallRepository.Update(hall);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var hall = await hallRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException<Hall>(id.ToString());

        hallRepository.Delete(hall);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<HallResponse>> SearchAvailableAsync(
        SearchAvailableHallsRequest request,
        CancellationToken cancellationToken = default)
    {
        var halls = await hallRepository.GetAvailableHallsAsync(
            request.StartTime,
            request.EndTime,
            request.Capacity,
            cancellationToken);

        return halls.Select(MapToResponse).ToList().AsReadOnly();
    }

    private async Task SynchronizeOptionsAsync(Hall hall, IEnumerable<Guid>? targetOptionIds, CancellationToken cancellationToken)
    {
        var desiredIds = new HashSet<Guid>(targetOptionIds ?? []);

        await optionRepository.GetByIdsOrThrowAsync(desiredIds, cancellationToken);

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

    private static HallResponse MapToResponse(Hall hall)
    {
        var options = hall.HallOptions
            .Where(ho => ho.Option is not null)
            .Select(ho => new OptionResponse(ho.Option.Id, ho.Option.Name, ho.Option.Price))
            .ToList();

        return new HallResponse(
            hall.Id,
            hall.Name,
            hall.Capacity,
            hall.BaseHourlyRate,
            options);
    }
}
