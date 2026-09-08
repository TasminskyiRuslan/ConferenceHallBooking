using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Interfaces;
using ConferenceHallBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHallBooking.Infrastructure.Repositories;

public class BookingRepository(AppDbContext context) : IBookingRepository
{
    public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Bookings
            .Include(b => b.BookingOptions)
                .ThenInclude(bo => bo.Option)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

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

    public async Task<int> GetBookingCountByHallIdAsync(
        Guid hallId,
        CancellationToken cancellationToken = default)
    {
        return await context.Bookings
            .CountAsync(b => b.HallId == hallId, cancellationToken);
    }

    public async Task AddAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        await context.Bookings.AddAsync(booking, cancellationToken);
    }

    public async Task<IReadOnlyList<Booking>> GetByDateRangeAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        return await context.Bookings
            .AsNoTracking()
            .Where(b => b.StartTime >= from && b.StartTime < to)
            .Include(b => b.Hall)
            .ToListAsync(cancellationToken);
    }
}
