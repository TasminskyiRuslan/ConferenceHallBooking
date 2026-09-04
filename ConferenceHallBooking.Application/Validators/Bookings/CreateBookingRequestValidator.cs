using ConferenceHallBooking.Application.DTOs.Bookings;
using FluentValidation;

namespace ConferenceHallBookingApi.ConferenceHallBooking.Application.Validators.Bookings;

public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(x => x.HallId)
            .NotEmpty().WithMessage("Hall ID is required.");

        RuleFor(x => x.StartTime)
            .GreaterThanOrEqualTo(DateTimeOffset.UtcNow)
            .WithMessage("Booking start time cannot be in the past.");

        RuleFor(x => x.DurationHours)
            .GreaterThan(0).WithMessage("Booking duration must be greater than zero.");
    }
}