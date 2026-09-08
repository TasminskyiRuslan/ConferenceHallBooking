using ConferenceHallBooking.Domain.Entities;

namespace ConferenceHallBooking.Domain.Interfaces;

/// <summary>
/// Repository for managing hall data access operations.
/// </summary>
public interface IHallRepository
{
    Task<Hall?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Hall>> GetAvailableHallsAsync(
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        int capacity,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Hall>> GetAllWithBookingsAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> IsOptionLinkedToAnyHallAsync(Guid optionId, CancellationToken cancellationToken = default);
    Task<int> GetHallCountByOptionIdAsync(Guid optionId, CancellationToken cancellationToken = default);
    Task AddAsync(Hall hall, CancellationToken cancellationToken = default);
    void Update(Hall hall);
    void Delete(Hall hall);
}