using ConferenceHallBooking.Application.Features.Bookings.Commands;
using FluentValidation;

namespace ConferenceHallBooking.Application.Validators.Bookings;

public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(x => x.HallId)
            .NotEmpty().WithMessage("Hall ID is required.");

        RuleFor(x => x.StartTime)
            .Must(time => time >= DateTimeOffset.UtcNow)
            .WithMessage("Booking start time cannot be in the past.");

        RuleFor(x => x.DurationHours)
            .GreaterThan(0).WithMessage("Booking duration must be greater than zero.");
    }
}
