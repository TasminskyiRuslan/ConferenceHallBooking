using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Interfaces;
using ConferenceHallBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHallBooking.Infrastructure.Repositories;

public class OptionRepository(AppDbContext context) : IOptionRepository
{
    public async Task<Option?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Options.FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<Option>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Options.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Option>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await context.Options
            .Where(o => ids.Contains(o.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<Option?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await context.Options
            .FirstOrDefaultAsync(o => o.Name == name, cancellationToken);
    }

    public async Task AddAsync(Option option, CancellationToken cancellationToken = default)
    {
        await context.Options.AddAsync(option, cancellationToken);
    }

    public void Update(Option option)
    {
        context.Options.Update(option);
    }

    public void Delete(Option option)
    {
        context.Options.Remove(option);
    }
}
