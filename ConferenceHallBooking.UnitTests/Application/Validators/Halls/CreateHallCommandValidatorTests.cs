using ConferenceHallBooking.Application.Features.Halls.Commands;
using ConferenceHallBooking.Application.Validators.Halls;
using FluentAssertions;

namespace ConferenceHallBooking.UnitTests.Application.Validators.Halls;

public class CreateHallCommandValidatorTests
{
    private readonly CreateHallCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WhenCommandIsValid_ShouldNotHaveValidationError()
    {
        var command = new CreateHallCommand("Test Hall", 10, 100m, null);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenNameIsEmpty_ShouldHaveValidationError()
    {
        var command = new CreateHallCommand(string.Empty, 10, 100m, null);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateHallCommand.Name));
    }

    [Fact]
    public async Task Validate_WhenNameIsWhitespace_ShouldHaveValidationError()
    {
        var command = new CreateHallCommand("   ", 10, 100m, null);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateHallCommand.Name));
    }

    [Fact]
    public async Task Validate_WhenNameExceeds100Characters_ShouldHaveValidationError()
    {
        var longName = new string('a', 101);
        var command = new CreateHallCommand(longName, 10, 100m, null);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateHallCommand.Name));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Validate_WhenCapacityIsInvalid_ShouldHaveValidationError(int invalidCapacity)
    {
        var command = new CreateHallCommand("Test Hall", invalidCapacity, 100m, null);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateHallCommand.Capacity));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-50.5)]
    public async Task Validate_WhenBaseHourlyRateIsInvalid_ShouldHaveValidationError(decimal invalidRate)
    {
        var command = new CreateHallCommand("Test Hall", 10, invalidRate, null);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateHallCommand.BaseHourlyRate));
    }
}
