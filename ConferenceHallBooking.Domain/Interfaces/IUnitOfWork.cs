namespace ConferenceHallBooking.Domain.Interfaces;

public interface IUnitOfWork
{
    IHallRepository Halls { get; }
    IBookingRepository Bookings { get; }
    IOptionRepository Options { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}