using ConferenceHallBooking.Application.DTOs.Halls;
using ConferenceHallBooking.Application.DTOs.Options;
using ConferenceHallBooking.Domain.Entities;

namespace ConferenceHallBooking.Application.Mappers;

public static class HallMapper
{
    public static HallResponse MapToResponse(Hall hall)
    {
        var options = hall.HallOptions
            .Where(ho => ho.Option is not null)
            .Select(ho => new OptionResponse(ho.Option.Id, ho.Option.Name, ho.Option.Price))
            .ToList();

        return new HallResponse(hall.Id, hall.Name, hall.Capacity, hall.BaseHourlyRate, options);
    }
}
