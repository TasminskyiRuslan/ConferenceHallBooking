using ConferenceHallBooking.Application.Features.Halls.Commands;
using ConferenceHallBooking.Application.Validators.Halls;
using FluentAssertions;

namespace ConferenceHallBooking.UnitTests.Application.Validators.Halls;

public class DeleteHallCommandValidatorTests
{
    private readonly DeleteHallCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WhenIdIsValid_ShouldNotHaveValidationError()
    {
        var command = new DeleteHallCommand(Guid.NewGuid());

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenIdIsEmpty_ShouldHaveValidationError()
    {
        var command = new DeleteHallCommand(Guid.Empty);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DeleteHallCommand.Id));
    }
}
