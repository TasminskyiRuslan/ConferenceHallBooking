using ConferenceHallBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ConferenceHallBooking.Infrastructure.Data;

public class DbInitializer : IDbInitializer
{
    private readonly AppDbContext _context;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(AppDbContext context, ILogger<DbInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Applying pending database migrations...");
            await _context.Database.MigrateAsync(cancellationToken);

            if (await _context.Halls.AnyAsync(cancellationToken))
            {
                _logger.LogInformation("Database already seeded. Skipping initial data population.");
                return;
            }

            _logger.LogInformation("Seeding initial database data...");

            var projector = new Option("Проєктор", 500m);
            var wifi = new Option("Wi-Fi", 300m);
            var sound = new Option("Звук", 700m);

            var hallA = new Hall("Зал А", 50, 2000m);
            var hallB = new Hall("Зал В", 100, 3500m);
            var hallC = new Hall("Зал С", 30, 1500m);

            await _context.Options.AddRangeAsync(new[] { projector, wifi, sound }, cancellationToken);
            await _context.Halls.AddRangeAsync(new[] { hallA, hallB, hallC }, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            hallA.AddOption(projector.Id);
            hallA.AddOption(wifi.Id);
            hallA.AddOption(sound.Id);

            hallB.AddOption(projector.Id);
            hallB.AddOption(wifi.Id);
            hallB.AddOption(sound.Id);

            hallC.AddOption(projector.Id);
            hallC.AddOption(wifi.Id);
            hallC.AddOption(sound.Id);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Database seeded successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}