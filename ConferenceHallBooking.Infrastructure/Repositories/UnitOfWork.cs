using ConferenceHallBooking.Domain.Interfaces;
using ConferenceHallBooking.Infrastructure.Data;

namespace ConferenceHallBooking.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public IHallRepository Halls { get; }
    public IBookingRepository Bookings { get; }
    public IOptionRepository Options { get; }

    public UnitOfWork(
        AppDbContext context,
        IHallRepository halls,
        IBookingRepository bookings,
        IOptionRepository options)
    {
        _context = context;
        Halls = halls;
        Bookings = bookings;
        Options = options;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}