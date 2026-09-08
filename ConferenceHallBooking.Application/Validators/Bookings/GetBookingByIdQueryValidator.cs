using ConferenceHallBooking.Application.Features.Bookings.Queries;
using FluentValidation;

namespace ConferenceHallBooking.Application.Validators.Bookings;

public class GetBookingByIdQueryValidator : AbstractValidator<GetBookingByIdQuery>
{
    public GetBookingByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Booking ID is required.");
    }
}
