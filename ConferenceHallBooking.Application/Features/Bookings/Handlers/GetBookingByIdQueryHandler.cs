using ConferenceHallBooking.Application.DTOs.Bookings;
using ConferenceHallBooking.Application.Features.Bookings.Queries;
using ConferenceHallBooking.Application.Mappers;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Bookings.Handlers;

public class GetBookingByIdQueryHandler(
    IBookingRepository bookingRepository,
    IHallRepository hallRepository) : IRequestHandler<GetBookingByIdQuery, BookingResponse>
{
    public async Task<BookingResponse> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException<Booking>(request.Id.ToString());

        var hall = await hallRepository.GetByIdAsync(booking.HallId, cancellationToken)
            ?? throw new NotFoundException<Hall>(booking.HallId.ToString());

        var domainOptions = booking.BookingOptions
            .Where(bo => bo.Option is not null)
            .Select(bo => bo.Option!)
            .ToList();

        return BookingMapper.MapToResponse(booking, hall, domainOptions);
    }
}
