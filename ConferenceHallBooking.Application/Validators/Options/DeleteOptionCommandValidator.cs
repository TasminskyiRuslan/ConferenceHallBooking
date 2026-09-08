using ConferenceHallBooking.Application.Features.Options.Commands;
using FluentValidation;

namespace ConferenceHallBooking.Application.Validators.Options;

public class DeleteOptionCommandValidator : AbstractValidator<DeleteOptionCommand>
{
    public DeleteOptionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Option ID is required.");
    }
}
