using ConferenceHallBooking.Domain.Entities;

namespace ConferenceHallBooking.Domain.Interfaces;

public interface IHallRepository
{
    Task<Hall?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Hall>> GetAvailableHallsAsync(
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        int capacity,
        CancellationToken cancellationToken = default);

    Task AddAsync(Hall hall, CancellationToken cancellationToken = default);
    void Update(Hall hall);
    void Delete(Hall hall);
}