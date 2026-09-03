using ConferenceHallBooking.Domain.Entities;

namespace ConferenceHallBooking.Domain.Interfaces;

public interface IBookingRepository
{
    Task<bool> HasOverlappingBookingAsync(
        long hallId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken = default);

    Task AddAsync(Booking booking, CancellationToken cancellationToken = default);
}