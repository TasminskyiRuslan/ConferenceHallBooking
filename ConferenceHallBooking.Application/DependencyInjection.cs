using ConferenceHallBooking.Application.Behaviours;
using ConferenceHallBooking.Application.Configuration;
using ConferenceHallBooking.Application.Interfaces.Bookings;
using ConferenceHallBooking.Application.Interfaces.Halls;
using ConferenceHallBooking.Application.Services.Bookings;
using ConferenceHallBooking.Application.Services.Halls;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

        services.AddScoped<IHallService, HallService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IPricingService, PricingService>();

        services.Configure<PricingSettings>(configuration.GetSection(PricingSettings.SectionName));

        return services;
    }
}
