using ConferenceHallBooking.Application.Features.Options.Queries;
using FluentValidation;

namespace ConferenceHallBooking.Application.Validators.Options;

public class GetOptionByIdQueryValidator : AbstractValidator<GetOptionByIdQuery>
{
    public GetOptionByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Option ID is required.");
    }
}
