using FluentValidation;

namespace ConferenceHallBooking.Application.Validators.Reports;

/// <summary>
/// Base validator for any query that has a date range (From/To).
/// </summary>
public abstract class DateRangeQueryValidator<T> : AbstractValidator<T>
    where T : IDateRangeQuery
{
    protected DateRangeQueryValidator()
    {
        RuleFor(x => x.To)
            .GreaterThan(x => x.From)
            .WithMessage("End date must be after start date.");

        RuleFor(x => x.From)
            .LessThanOrEqualTo(DateTimeOffset.UtcNow)
            .WithMessage("Start date cannot be in the future.");
    }
}

public interface IDateRangeQuery
{
    DateTimeOffset From { get; }
    DateTimeOffset To { get; }
}
