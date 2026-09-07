using ConferenceHallBooking.Application.Features.Halls.Commands;
using FluentValidation;

namespace ConferenceHallBooking.Application.Validators.Halls;

public class DeleteHallCommandValidator : AbstractValidator<DeleteHallCommand>
{
    public DeleteHallCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Hall ID is required.");
    }
}
