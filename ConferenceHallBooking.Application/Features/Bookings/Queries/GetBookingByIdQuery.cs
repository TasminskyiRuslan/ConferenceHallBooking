using ConferenceHallBooking.Application.DTOs.Bookings;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Bookings.Queries;

public record GetBookingByIdQuery(Guid Id) : IRequest<BookingResponse>;
