using ConferenceHallBooking.Application.Features.Options.Commands;
using FluentValidation;

namespace ConferenceHallBooking.Application.Validators.Options;

public class CreateOptionCommandValidator : AbstractValidator<CreateOptionCommand>
{
    public CreateOptionCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Option name is required.")
            .MaximumLength(100).WithMessage("Option name must not exceed 100 characters.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Option price cannot be negative.");
    }
}
