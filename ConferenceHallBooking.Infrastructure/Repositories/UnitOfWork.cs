using ConferenceHallBooking.Domain.Interfaces;
using ConferenceHallBooking.Infrastructure.Data;

namespace ConferenceHallBooking.Infrastructure.Repositories;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}
