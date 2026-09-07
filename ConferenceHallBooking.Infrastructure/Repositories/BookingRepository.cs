using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Interfaces;
using ConferenceHallBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHallBooking.Infrastructure.Repositories;

public class BookingRepository(AppDbContext context) : IBookingRepository
{
    public async Task<bool> HasOverlappingBookingAsync(
        Guid hallId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken = default)
    {
        return await context.Bookings
            .AnyAsync(b => b.HallId == hallId
                        && startTime < b.EndTime
                        && endTime > b.StartTime, cancellationToken);
    }

    public async Task AddAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        await context.Bookings.AddAsync(booking, cancellationToken);
    }
}
