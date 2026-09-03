using Microsoft.EntityFrameworkCore;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Interfaces;
using ConferenceHallBooking.Infrastructure.Data;

namespace ConferenceHallBooking.Infrastructure.Repositories;

public class HallRepository : IHallRepository
{
    private readonly AppDbContext _context;

    public HallRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Hall?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Halls
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
        return await _context.Halls
            .AsNoTracking()
            .Where(h => h.Capacity >= capacity)
            .Where(h => !h.Bookings.Any(b => startTime < b.EndTime && endTime > b.StartTime))
            .Include(h => h.HallOptions)
                .ThenInclude(ho => ho.Option)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Hall hall, CancellationToken cancellationToken = default)
    {
        await _context.Halls.AddAsync(hall, cancellationToken);
    }

    public void Update(Hall hall)
    {
        _context.Halls.Update(hall);
    }

    public void Delete(Hall hall)
    {
        _context.Halls.Remove(hall);
    }
}