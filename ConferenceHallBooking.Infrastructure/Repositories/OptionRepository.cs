using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Interfaces;
using ConferenceHallBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHallBooking.Infrastructure.Repositories;

public class OptionRepository : IOptionRepository
{
    private readonly AppDbContext _context;

    public OptionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Option>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await _context.Options
            .Where(o => ids.Contains(o.Id))
            .ToListAsync(cancellationToken);
    }
}