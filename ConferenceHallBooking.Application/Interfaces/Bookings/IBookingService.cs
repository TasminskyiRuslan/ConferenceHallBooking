using ConferenceHallBooking.Application.DTOs.Bookings;

namespace ConferenceHallBooking.Application.Interfaces.Bookings;

public interface IBookingService
{
    Task<BookingResponse> CreateAsync(
        CreateBookingRequest request,
        CancellationToken cancellationToken = default);
}