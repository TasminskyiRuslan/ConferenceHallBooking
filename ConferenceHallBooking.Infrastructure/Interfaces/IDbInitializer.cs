namespace ConferenceHallBooking.Infrastructure.Data;

public interface IDbInitializer
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}