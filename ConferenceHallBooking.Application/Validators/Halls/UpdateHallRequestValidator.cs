using ConferenceHallBooking.Application.DTOs.Halls;
using FluentValidation;

namespace ConferenceHallBookingApi.ConferenceHallBooking.Application.Validators.Halls;

public class UpdateHallRequestValidator : AbstractValidator<UpdateHallRequest>
{
    public UpdateHallRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Hall name is required.")
            .MaximumLength(100).WithMessage("Hall name must not exceed 100 characters.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than zero.");

        RuleFor(x => x.BaseHourlyRate)
            .GreaterThanOrEqualTo(0).WithMessage("Base hourly rate cannot be negative.");
    }
}