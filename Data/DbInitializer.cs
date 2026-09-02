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

        var projector = new Service { Name = "Проєктор", Price = 500m };
        var wifi = new Service { Name = "Wi-Fi", Price = 300m };
        var sound = new Service { Name = "Звук", Price = 700m };

        await context.Services.AddRangeAsync(projector, wifi, sound);
        await context.SaveChangesAsync();

        var hallA = new Hall { Name = "Зал А", Capacity = 50, BaseHourlyRate = 2000m };
        var hallB = new Hall { Name = "Зал В", Capacity = 100, BaseHourlyRate = 3500m };
        var hallC = new Hall { Name = "Зал С", Capacity = 30, BaseHourlyRate = 1500m };

        await context.Halls.AddRangeAsync(hallA, hallB, hallC);
        await context.SaveChangesAsync();

        var hallServices = new List<HallService>
        {
            new() { HallId = hallA.Id, ServiceId = projector.Id },
            new() { HallId = hallA.Id, ServiceId = wifi.Id },
            new() { HallId = hallA.Id, ServiceId = sound.Id },

            new() { HallId = hallB.Id, ServiceId = projector.Id },
            new() { HallId = hallB.Id, ServiceId = wifi.Id },
            new() { HallId = hallB.Id, ServiceId = sound.Id },

            new() { HallId = hallC.Id, ServiceId = projector.Id },
            new() { HallId = hallC.Id, ServiceId = wifi.Id },
            new() { HallId = hallC.Id, ServiceId = sound.Id }
        };

        await context.HallServices.AddRangeAsync(hallServices);
        await context.SaveChangesAsync();
    }
}