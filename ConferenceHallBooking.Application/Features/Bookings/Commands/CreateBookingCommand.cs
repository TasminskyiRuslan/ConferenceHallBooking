using ConferenceHallBooking.Application.DTOs.Bookings;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Bookings.Commands;

public record CreateBookingCommand(
    Guid HallId,
    DateTimeOffset StartTime,
    decimal DurationHours,
    List<Guid>? OptionIds) : IRequest<BookingResponse>;
