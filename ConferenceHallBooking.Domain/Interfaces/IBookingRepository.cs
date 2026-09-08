using ConferenceHallBooking.Domain.Entities;

namespace ConferenceHallBooking.Domain.Interfaces;

/// <summary>
/// Repository for managing booking data access operations.
/// </summary>
public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> HasOverlappingBookingAsync(
        Guid hallId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken = default);

    Task<int> GetBookingCountByHallIdAsync(
        Guid hallId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Booking>> GetByDateRangeAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);

    Task AddAsync(Booking booking, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetUsedOptionIdsByHallAsync(
        Guid hallId,
        CancellationToken cancellationToken = default);

    Task<bool> IsOptionUsedInAnyBookingAsync(Guid optionId, CancellationToken cancellationToken = default);
    Task<int> GetBookingCountByOptionIdAsync(Guid optionId, CancellationToken cancellationToken = default);
}
