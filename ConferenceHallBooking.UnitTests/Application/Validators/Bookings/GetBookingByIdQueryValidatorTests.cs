using ConferenceHallBooking.Application.Features.Bookings.Queries;
using ConferenceHallBooking.Application.Validators.Bookings;
using FluentAssertions;

namespace ConferenceHallBooking.UnitTests.Application.Validators.Bookings;

public class GetBookingByIdQueryValidatorTests
{
    private readonly GetBookingByIdQueryValidator _validator = new();

    [Fact]
    public async Task Validate_WhenIdIsValid_ShouldNotHaveValidationError()
    {
        var query = new GetBookingByIdQuery(Guid.NewGuid());

        var result = await _validator.ValidateAsync(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenIdIsEmpty_ShouldHaveValidationError()
    {
        var query = new GetBookingByIdQuery(Guid.Empty);

        var result = await _validator.ValidateAsync(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetBookingByIdQuery.Id));
    }
}
