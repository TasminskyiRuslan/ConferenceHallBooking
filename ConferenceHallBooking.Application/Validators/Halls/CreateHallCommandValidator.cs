using ConferenceHallBooking.Application.Features.Halls.Commands;
using FluentValidation;

namespace ConferenceHallBooking.Application.Validators.Halls;

public class CreateHallCommandValidator : AbstractValidator<CreateHallCommand>
{
    public CreateHallCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Hall name is required.")
            .MaximumLength(100).WithMessage("Hall name must not exceed 100 characters.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than zero.");

        RuleFor(x => x.BaseHourlyRate)
            .GreaterThan(0).WithMessage("Base hourly rate must be greater than zero.");
    }
}
