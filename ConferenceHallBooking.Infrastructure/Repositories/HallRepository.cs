using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Interfaces;
using ConferenceHallBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHallBooking.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for managing <see cref="Hall"/> entity data access.
/// Provides server-side queries to avoid N+1 issues.
/// </summary>
public class HallRepository(AppDbContext context) : IHallRepository
{
    public async Task<Hall?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Halls
            .Include(h => h.HallOptions)
                .ThenInclude(ho => ho.Option)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await context.Halls.AnyAsync(h => h.Name == name, cancellationToken);
    }

    public async Task<bool> IsOptionLinkedToAnyHallAsync(Guid optionId, CancellationToken cancellationToken = default)
    {
        return await context.HallOptions.AnyAsync(ho => ho.OptionId == optionId, cancellationToken);
    }

    public async Task<int> GetHallCountByOptionIdAsync(Guid optionId, CancellationToken cancellationToken = default)
    {
        return await context.HallOptions.CountAsync(ho => ho.OptionId == optionId, cancellationToken);
    }

    public async Task<IReadOnlyList<Hall>> GetAvailableHallsAsync(
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        int capacity,
        CancellationToken cancellationToken = default)
    {
        var bookedHallIds = await context.Bookings
            .AsNoTracking()
            .Where(b => startTime < b.EndTime && endTime > b.StartTime)
            .Select(b => b.HallId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return await context.Halls
            .AsNoTracking()
            .Where(h => h.Capacity >= capacity && !bookedHallIds.Contains(h.Id))
            .Include(h => h.HallOptions)
                .ThenInclude(ho => ho.Option)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Hall hall, CancellationToken cancellationToken = default)
    {
        await context.Halls.AddAsync(hall, cancellationToken);
    }

    public void Update(Hall hall)
    {
        context.Halls.Update(hall);
    }

    public void Delete(Hall hall)
    {
        context.Halls.Remove(hall);
    }

    public async Task<IReadOnlyList<Hall>> GetAllWithBookingsAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        return await context.Halls
            .AsNoTracking()
            .Include(h => h.Bookings.Where(b => b.StartTime >= from && b.StartTime < to))
            .ToListAsync(cancellationToken);
    }
}
