using ConferenceHallBooking.Application.DTOs.Bookings;
using ConferenceHallBooking.Application.DTOs.Options;
using ConferenceHallBooking.Domain.Entities;

namespace ConferenceHallBooking.Application.Mappers;

public static class BookingMapper
{
    public static BookingResponse MapToResponse(
        Booking booking,
        Hall hall,
        IReadOnlyCollection<Option> options,
        PricingResult pricing,
        decimal durationHours)
    {
        var optionResponses = options
            .Select(o => new OptionResponse(o.Id, o.Name, o.Price))
            .ToList();

        var storedOptionsCost = booking.BookingOptions
            .Sum(bo => bo.PriceAtBooking);

        return new BookingResponse(
            booking.Id,
            hall.Id,
            hall.Name,
            hall.Capacity,
            hall.BaseHourlyRate,
            booking.StartTime,
            booking.EndTime,
            durationHours,
            optionResponses,
            pricing.HallCost,
            storedOptionsCost,
            booking.TotalPrice);
    }

    public static BookingResponse MapToResponse(
        Booking booking,
        Hall hall,
        IReadOnlyCollection<Option> options)
    {
        var durationHours = (decimal)(booking.EndTime - booking.StartTime).TotalHours;
        var storedOptionsCost = booking.BookingOptions
            .Sum(bo => bo.PriceAtBooking);
        var hallCost = booking.TotalPrice - storedOptionsCost;

        var optionResponses = options
            .Select(o => new OptionResponse(o.Id, o.Name, o.Price))
            .ToList();

        return new BookingResponse(
            booking.Id,
            hall.Id,
            hall.Name,
            hall.Capacity,
            hall.BaseHourlyRate,
            booking.StartTime,
            booking.EndTime,
            durationHours,
            optionResponses,
            hallCost,
            storedOptionsCost,
            booking.TotalPrice);
    }
}
