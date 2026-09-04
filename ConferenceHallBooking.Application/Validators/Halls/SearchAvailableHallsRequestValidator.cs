using ConferenceHallBooking.Application.DTOs.Halls;
using FluentValidation;

namespace ConferenceHallBookingApi.ConferenceHallBooking.Application.Validators.Halls;

public class SearchAvailableHallsRequestValidator : AbstractValidator<SearchAvailableHallsRequest>
{
    public SearchAvailableHallsRequestValidator()
    {
        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Requested capacity must be greater than zero.");

        RuleFor(x => x.StartTime)
            .GreaterThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Start time cannot be in the past.");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("End time must be greater than start time.");
    }
}