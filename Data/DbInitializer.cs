using ConferenceHallBookingApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHallBookingApi.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        if (await context.Halls.AnyAsync())
        {
            return;
        }

        var projector = new Option { Name = "Проєктор", Price = 500m };
        var wifi = new Option { Name = "Wi-Fi", Price = 300m };
        var sound = new Option { Name = "Звук", Price = 700m };

        await context.Options.AddRangeAsync(projector, wifi, sound);
        await context.SaveChangesAsync();

        var hallA = new Hall { Name = "Зал А", Capacity = 50, BaseHourlyRate = 2000m };
        var hallB = new Hall { Name = "Зал В", Capacity = 100, BaseHourlyRate = 3500m };
        var hallC = new Hall { Name = "Зал С", Capacity = 30, BaseHourlyRate = 1500m };

        await context.Halls.AddRangeAsync(hallA, hallB, hallC);
        await context.SaveChangesAsync();

        var hallOptions = new List<HallOption>
        {
            new() { HallId = hallA.Id, OptionId = projector.Id },
            new() { HallId = hallA.Id, OptionId = wifi.Id },
            new() { HallId = hallA.Id, OptionId = sound.Id },

            new() { HallId = hallB.Id, OptionId = projector.Id },
            new() { HallId = hallB.Id, OptionId = wifi.Id },
            new() { HallId = hallB.Id, OptionId = sound.Id },

            new() { HallId = hallC.Id, OptionId = projector.Id },
            new() { HallId = hallC.Id, OptionId = wifi.Id },
            new() { HallId = hallC.Id, OptionId = sound.Id }
        };

        await context.HallOptions.AddRangeAsync(hallOptions);
        await context.SaveChangesAsync();
    }
}