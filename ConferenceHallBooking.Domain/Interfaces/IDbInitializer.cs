namespace ConferenceHallBooking.Domain.Interfaces;

public interface IDbInitializer
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
