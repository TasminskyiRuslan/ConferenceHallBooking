using ConferenceHallBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHallBooking.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        if (await context.Halls.AnyAsync())
        {
            return;
        }

        var projector = new Option("Проєктор", 500m);
        var wifi = new Option("Wi-Fi", 300m);
        var sound = new Option("Звук", 700m);

        await context.Options.AddRangeAsync(projector, wifi, sound);
        await context.SaveChangesAsync();

        var hallA = new Hall("Зал А", 50, 2000m);
        var hallB = new Hall("Зал В", 100, 3500m);
        var hallC = new Hall("Зал С", 30, 1500m);

        await context.Halls.AddRangeAsync(hallA, hallB, hallC);
        await context.SaveChangesAsync();

        hallA.AddOption(projector.Id);
        hallA.AddOption(wifi.Id);
        hallA.AddOption(sound.Id);

        hallB.AddOption(projector.Id);
        hallB.AddOption(wifi.Id);
        hallB.AddOption(sound.Id);

        hallC.AddOption(projector.Id);
        hallC.AddOption(wifi.Id);
        hallC.AddOption(sound.Id);

        await context.SaveChangesAsync();
    }
}