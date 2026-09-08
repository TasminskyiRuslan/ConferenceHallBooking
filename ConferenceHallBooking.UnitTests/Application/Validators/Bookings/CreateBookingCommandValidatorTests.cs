using ConferenceHallBooking.Application.Features.Bookings.Commands;
using ConferenceHallBooking.Application.Validators.Bookings;
using FluentAssertions;

namespace ConferenceHallBooking.UnitTests.Application.Validators.Bookings;

public class CreateBookingCommandValidatorTests
{
    private readonly CreateBookingCommandValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    [InlineData(-0.5)]
    public async Task Validate_WhenDurationIsInvalid_ShouldHaveValidationError(decimal invalidDuration)
    {
        var command = new CreateBookingCommand(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(1),
            invalidDuration,
            null);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateBookingCommand.DurationHours));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2.5)]
    [InlineData(8)]
    public async Task Validate_WhenDurationIsValid_ShouldNotHaveDurationError(decimal validDuration)
    {
        var command = new CreateBookingCommand(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(1),
            validDuration,
            null);

        var result = await _validator.ValidateAsync(command);

        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CreateBookingCommand.DurationHours));
    }

    [Fact]
    public async Task Validate_WhenHallIdIsEmpty_ShouldHaveValidationError()
    {
        var command = new CreateBookingCommand(
            Guid.Empty,
            DateTimeOffset.UtcNow.AddDays(1),
            2m,
            null);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateBookingCommand.HallId));
    }

    [Fact]
    public async Task Validate_WhenStartTimeIsInThePast_ShouldHaveValidationError()
    {
        var command = new CreateBookingCommand(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(-1),
            2m,
            null);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateBookingCommand.StartTime));
    }
}
