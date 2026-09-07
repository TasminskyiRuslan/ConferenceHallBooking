using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Interfaces;
using ConferenceHallBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHallBooking.Infrastructure.Repositories;

public class HallRepository(AppDbContext context) : IHallRepository
{
    public async Task<Hall?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Halls
            .Include(h => h.HallOptions)
                .ThenInclude(ho => ho.Option)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Hall>> GetAvailableHallsAsync(
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        int capacity,
        CancellationToken cancellationToken = default)
    {
        return await context.Halls
            .AsNoTracking()
            .Where(h => h.Capacity >= capacity)
            .Where(h => !h.Bookings.Any(b => startTime < b.EndTime && endTime > b.StartTime))
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
}
