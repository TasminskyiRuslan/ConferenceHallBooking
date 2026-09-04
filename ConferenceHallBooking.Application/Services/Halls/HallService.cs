using ConferenceHallBooking.Application.DTOs.Halls;
using ConferenceHallBooking.Application.DTOs.Options;
using ConferenceHallBooking.Application.Exceptions;
using ConferenceHallBooking.Application.Interfaces.Halls;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ConferenceHallBooking.Application.Services.Halls;

public class HallService : IHallService
{
    private readonly IHallRepository _hallRepository;
    private readonly IOptionRepository _optionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<HallService> _logger;

    public HallService(
        IHallRepository hallRepository,
        IOptionRepository optionRepository,
        IUnitOfWork unitOfWork,
        ILogger<HallService> logger)
    {
        _hallRepository = hallRepository;
        _optionRepository = optionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Guid> CreateAsync(CreateHallRequest request, CancellationToken cancellationToken = default)
    {
        var targetOptionIds = (request.OptionIds ?? []).Distinct().ToList();

        if (targetOptionIds.Count > 0)
        {
            await GetOptionsOrThrowAsync(targetOptionIds, cancellationToken);
        }

        var hall = new Hall(request.Name, request.Capacity, request.BaseHourlyRate);

        foreach (var optionId in targetOptionIds)
        {
            hall.AddOption(optionId);
        }

        await _hallRepository.AddAsync(hall, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Conference hall '{HallName}' with ID {HallId} was successfully created.", hall.Name, hall.Id);

        return hall.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateHallRequest request, CancellationToken cancellationToken = default)
    {
        var hall = await GetHallByIdOrThrowAsync(id, cancellationToken);

        hall.Update(request.Name, request.Capacity, request.BaseHourlyRate);

        await SynchronizeHallOptionsAsync(hall, request.OptionIds, cancellationToken);

        _hallRepository.Update(hall);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Conference hall {HallId} details were successfully updated.", hall.Id);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var hall = await GetHallByIdOrThrowAsync(id, cancellationToken);

        _hallRepository.Delete(hall);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Conference hall {HallId} was successfully deleted.", id);
    }

    public async Task<IReadOnlyCollection<HallResponse>> SearchAvailableAsync(
        SearchAvailableHallsRequest request,
        CancellationToken cancellationToken = default)
    {
        var availableHalls = (await _hallRepository.GetAvailableHallsAsync(
            request.StartTime,
            request.EndTime,
            request.Capacity,
            cancellationToken)).ToList();

        _logger.LogInformation(
            "Found {Count} available conference hall(s) for capacity >= {Capacity} between {StartTime} and {EndTime}.",
            availableHalls.Count, request.Capacity, request.StartTime, request.EndTime);

        return availableHalls.Select(MapToResponse).ToList().AsReadOnly();
    }

    private async Task<Hall> GetHallByIdOrThrowAsync(Guid hallId, CancellationToken cancellationToken)
    {
        var hall = await _hallRepository.GetByIdAsync(hallId, cancellationToken);

        if (hall is null)
        {
            _logger.LogWarning("Conference hall with ID {HallId} was not found.", hallId);
            throw new NotFoundException($"Conference hall with ID '{hallId}' was not found.");
        }

        return hall;
    }

    private async Task<IReadOnlyList<Option>> GetOptionsOrThrowAsync(
        IReadOnlyCollection<Guid> distinctOptionIds,
        CancellationToken cancellationToken)
    {
        var existingOptions = (await _optionRepository.GetByIdsAsync(distinctOptionIds, cancellationToken)).ToList();

        if (existingOptions.Count != distinctOptionIds.Count)
        {
            _logger.LogWarning("Attempted to access one or more non-existent options.");
            throw new BusinessRuleException("One or more specified options do not exist.");
        }

        return existingOptions;
    }

    private async Task SynchronizeHallOptionsAsync(
        Hall hall,
        IEnumerable<Guid>? targetOptionIds,
        CancellationToken cancellationToken)
    {
        var desiredIds = (targetOptionIds ?? []).Distinct().ToHashSet();

        if (desiredIds.Count > 0)
        {
            await GetOptionsOrThrowAsync(desiredIds, cancellationToken);
        }

        var currentIds = hall.HallOptions.Select(ho => ho.OptionId).ToList();

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
            .Where(ho => ho.Option != null)
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