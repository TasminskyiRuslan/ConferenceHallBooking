using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Interfaces;
using ConferenceHallBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHallBooking.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for managing <see cref="Booking"/> entity data access.
/// Provides server-side queries to avoid N+1 issues.
/// </summary>
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
            .AnyAsync(b => b.HallId == hallId && startTime < b.EndTime && endTime > b.StartTime, cancellationToken);
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
            .Include(b => b.Hall)
            .Where(b => b.StartTime >= from && b.StartTime < to)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> GetUsedOptionIdsByHallAsync(
        Guid hallId,
        CancellationToken cancellationToken = default)
    {
        return await context.BookingOptions
            .AsNoTracking()
            .Where(bo => bo.Booking.HallId == hallId)
            .Select(bo => bo.OptionId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsOptionUsedInAnyBookingAsync(Guid optionId, CancellationToken cancellationToken = default)
    {
        return await context.BookingOptions
            .AsNoTracking()
            .AnyAsync(bo => bo.OptionId == optionId, cancellationToken);
    }

    public async Task<int> GetBookingCountByOptionIdAsync(Guid optionId, CancellationToken cancellationToken = default)
    {
        return await context.BookingOptions
            .AsNoTracking()
            .CountAsync(bo => bo.OptionId == optionId, cancellationToken);
    }
}
