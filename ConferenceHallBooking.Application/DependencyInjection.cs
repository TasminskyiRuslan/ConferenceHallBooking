using ConferenceHallBooking.Application.Configuration;
using ConferenceHallBooking.Application.Interfaces.Bookings;
using ConferenceHallBooking.Application.Interfaces.Halls;
using ConferenceHallBooking.Application.Services.Bookings;
using ConferenceHallBooking.Application.Services.Halls;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IHallService, HallService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IPricingService, PricingService>();

        services.Configure<PricingSettings>(configuration.GetSection(PricingSettings.SectionName));

        return services;
    }
}
