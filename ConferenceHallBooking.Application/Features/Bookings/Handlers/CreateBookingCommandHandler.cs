using ConferenceHallBooking.Application.DTOs.Bookings;
using ConferenceHallBooking.Application.Features.Bookings.Commands;
using ConferenceHallBooking.Application.Interfaces.Bookings;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Bookings.Handlers;

public class CreateBookingCommandHandler(IBookingService bookingService)
    : IRequestHandler<CreateBookingCommand, BookingResponse>
{
    public async Task<BookingResponse> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var createRequest = new CreateBookingRequest(
            request.HallId,
            request.StartTime,
            request.DurationHours,
            request.OptionIds);

        return await bookingService.CreateAsync(createRequest, cancellationToken);
    }
}
