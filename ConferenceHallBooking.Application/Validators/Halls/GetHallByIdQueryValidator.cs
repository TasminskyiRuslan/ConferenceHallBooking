using ConferenceHallBooking.Application.Features.Halls.Queries;
using FluentValidation;

namespace ConferenceHallBooking.Application.Validators.Halls;

public class GetHallByIdQueryValidator : AbstractValidator<GetHallByIdQuery>
{
    public GetHallByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Hall ID is required.");
    }
}
