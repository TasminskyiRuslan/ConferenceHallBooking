using ConferenceHallBooking.Application.Features.Halls.Queries;
using ConferenceHallBooking.Application.Validators.Halls;
using FluentAssertions;

namespace ConferenceHallBooking.UnitTests.Application.Validators.Halls;

public class GetHallByIdQueryValidatorTests
{
    private readonly GetHallByIdQueryValidator _validator = new();

    [Fact]
    public async Task Validate_WhenIdIsValid_ShouldNotHaveValidationError()
    {
        var query = new GetHallByIdQuery(Guid.NewGuid());

        var result = await _validator.ValidateAsync(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenIdIsEmpty_ShouldHaveValidationError()
    {
        var query = new GetHallByIdQuery(Guid.Empty);

        var result = await _validator.ValidateAsync(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetHallByIdQuery.Id));
    }
}
