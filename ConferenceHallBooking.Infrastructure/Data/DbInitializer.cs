using ConferenceHallBooking.Application.Interfaces;
using ConferenceHallBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ConferenceHallBooking.Infrastructure.Data;

public class DbInitializer(AppDbContext context, ILogger<DbInitializer> logger) : IDbInitializer
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Applying pending database migrations...");
            await context.Database.MigrateAsync(cancellationToken);

            if (await context.Halls.AnyAsync(cancellationToken))
            {
                logger.LogInformation("Database already seeded. Skipping initial data population.");
                return;
            }

            logger.LogInformation("Seeding initial database data...");

            var projector = new Option("Проєктор", 500m);
            var wifi = new Option("Wi-Fi", 300m);
            var sound = new Option("Звук", 700m);

            var hallA = new Hall("Зал А", 50, 2000m);
            var hallB = new Hall("Зал B", 100, 3500m);
            var hallC = new Hall("Зал C", 30, 1500m);

            await context.Options.AddRangeAsync([projector, wifi, sound], cancellationToken);
            await context.Halls.AddRangeAsync([hallA, hallB, hallC], cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            hallA.AddOption(projector.Id);
            hallA.AddOption(wifi.Id);
            hallA.AddOption(sound.Id);

            hallB.AddOption(projector.Id);
            hallB.AddOption(wifi.Id);
            hallB.AddOption(sound.Id);

            hallC.AddOption(projector.Id);
            hallC.AddOption(wifi.Id);
            hallC.AddOption(sound.Id);

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Database seeded successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
