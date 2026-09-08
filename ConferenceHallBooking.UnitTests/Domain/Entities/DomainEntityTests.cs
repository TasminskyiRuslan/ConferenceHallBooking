using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using FluentAssertions;

namespace ConferenceHallBooking.UnitTests.Domain.Entities;

public class DomainEntityTests
{
    #region Hall

    [Fact]
    public void Hall_Constructor_WithValidData_ShouldSetPropertiesCorrectly()
    {
        var hall = new Hall("Conference Room A", 50, 100m);

        hall.Id.Should().NotBe(Guid.Empty);
        hall.Name.Should().Be("Conference Room A");
        hall.Capacity.Should().Be(50);
        hall.BaseHourlyRate.Should().Be(100m);
    }

    [Fact]
    public void Hall_Constructor_WithEmptyName_ShouldThrowInvalidEntityFieldException()
    {
        var act = () => new Hall("", 50, 100m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(Hall.Name));
    }

    [Fact]
    public void Hall_Constructor_WithWhitespaceName_ShouldThrowInvalidEntityFieldException()
    {
        var act = () => new Hall("   ", 50, 100m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(Hall.Name));
    }

    [Fact]
    public void Hall_Constructor_WithZeroCapacity_ShouldThrowInvalidEntityFieldException()
    {
        var act = () => new Hall("Conference Room A", 0, 100m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(Hall.Capacity));
    }

    [Fact]
    public void Hall_Constructor_WithNegativeCapacity_ShouldThrowInvalidEntityFieldException()
    {
        var act = () => new Hall("Conference Room A", -1, 100m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(Hall.Capacity));
    }

    [Fact]
    public void Hall_Constructor_WithZeroBaseHourlyRate_ShouldThrowInvalidEntityFieldException()
    {
        var act = () => new Hall("Conference Room A", 50, 0m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(Hall.BaseHourlyRate));
    }

    [Fact]
    public void Hall_Constructor_WithNegativeBaseHourlyRate_ShouldThrowInvalidEntityFieldException()
    {
        var act = () => new Hall("Conference Room A", 50, -10m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(Hall.BaseHourlyRate));
    }

    [Fact]
    public void Hall_Update_WithValidData_ShouldChangeProperties()
    {
        var hall = new Hall("Conference Room A", 50, 100m);

        hall.Update("Conference Room B", 100, 200m);

        hall.Name.Should().Be("Conference Room B");
        hall.Capacity.Should().Be(100);
        hall.BaseHourlyRate.Should().Be(200m);
    }

    [Fact]
    public void Hall_Update_WithEmptyName_ShouldThrowInvalidEntityFieldException()
    {
        var hall = new Hall("Conference Room A", 50, 100m);

        var act = () => hall.Update("", 50, 100m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(Hall.Name));
    }

    [Fact]
    public void Hall_AddOption_ShouldAddToHallOptions()
    {
        var hall = new Hall("Conference Room A", 50, 100m);
        var optionId = Guid.NewGuid();

        hall.AddOption(optionId);

        hall.HallOptions.Should().ContainSingle(o => o.OptionId == optionId);
    }

    [Fact]
    public void Hall_AddOption_WithDuplicateId_ShouldNotAddDuplicate()
    {
        var hall = new Hall("Conference Room A", 50, 100m);
        var optionId = Guid.NewGuid();

        hall.AddOption(optionId);
        hall.AddOption(optionId);

        hall.HallOptions.Should().ContainSingle(o => o.OptionId == optionId);
    }

    [Fact]
    public void Hall_RemoveOption_ShouldRemoveFromHallOptions()
    {
        var hall = new Hall("Conference Room A", 50, 100m);
        var optionId = Guid.NewGuid();
        hall.AddOption(optionId);

        hall.RemoveOption(optionId);

        hall.HallOptions.Should().BeEmpty();
    }

    #endregion

    #region Booking

    [Fact]
    public void Booking_Constructor_WithValidData_ShouldSetPropertiesCorrectly()
    {
        var startTime = DateTimeOffset.UtcNow;
        var endTime = startTime.AddHours(3);

        var booking = new Booking(Guid.NewGuid(), startTime, endTime, 300m);

        booking.Id.Should().NotBe(Guid.Empty);
        booking.StartTime.Should().Be(startTime);
        booking.EndTime.Should().Be(endTime);
        booking.TotalPrice.Should().Be(300m);
        booking.CreatedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Booking_Constructor_WithEndTimeBeforeStartTime_ShouldThrowInvalidBookingTimeException()
    {
        var startTime = DateTimeOffset.UtcNow;
        var endTime = startTime.AddHours(-1);

        var act = () => new Booking(Guid.NewGuid(), startTime, endTime, 300m);

        act.Should().Throw<InvalidBookingTimeException>();
    }

    [Fact]
    public void Booking_Constructor_WithEndTimeEqualToStartTime_ShouldThrowInvalidBookingTimeException()
    {
        var time = DateTimeOffset.UtcNow;

        var act = () => new Booking(Guid.NewGuid(), time, time, 300m);

        act.Should().Throw<InvalidBookingTimeException>();
    }

    [Fact]
    public void Booking_Constructor_WithTotalPriceZero_ShouldThrowInvalidEntityFieldException()
    {
        var startTime = DateTimeOffset.UtcNow;
        var endTime = startTime.AddHours(3);

        var act = () => new Booking(Guid.NewGuid(), startTime, endTime, 0m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(Booking.TotalPrice));
    }

    [Fact]
    public void Booking_Constructor_WithTotalPriceNegative_ShouldThrowInvalidEntityFieldException()
    {
        var startTime = DateTimeOffset.UtcNow;
        var endTime = startTime.AddHours(3);

        var act = () => new Booking(Guid.NewGuid(), startTime, endTime, -100m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(Booking.TotalPrice));
    }

    [Fact]
    public void Booking_Constructor_WithOptions_ShouldStoreInBookingOptions()
    {
        var startTime = DateTimeOffset.UtcNow;
        var endTime = startTime.AddHours(3);
        var options = new List<BookingOption>
        {
            new(Guid.NewGuid(), 50m),
            new(Guid.NewGuid(), 30m)
        };

        var booking = new Booking(Guid.NewGuid(), startTime, endTime, 380m, options);

        booking.BookingOptions.Should().HaveCount(2);
    }

    [Fact]
    public void Booking_Constructor_WithoutOptions_ShouldCreateEmptyBookingOptions()
    {
        var startTime = DateTimeOffset.UtcNow;
        var endTime = startTime.AddHours(3);

        var booking = new Booking(Guid.NewGuid(), startTime, endTime, 300m);

        booking.BookingOptions.Should().BeEmpty();
    }

    [Fact]
    public void Booking_Constructor_ShouldSetHallId()
    {
        var hallId = Guid.NewGuid();
        var startTime = DateTimeOffset.UtcNow;
        var endTime = startTime.AddHours(3);

        var booking = new Booking(hallId, startTime, endTime, 300m);

        booking.HallId.Should().Be(hallId);
    }

    #endregion

    #region Option

    [Fact]
    public void Option_Constructor_WithValidData_ShouldSetPropertiesCorrectly()
    {
        var option = new Option("Projector", 50m);

        option.Id.Should().NotBe(Guid.Empty);
        option.Name.Should().Be("Projector");
        option.Price.Should().Be(50m);
    }

    [Fact]
    public void Option_Constructor_WithEmptyName_ShouldThrowInvalidEntityFieldException()
    {
        var act = () => new Option("", 50m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(Option.Name));
    }

    [Fact]
    public void Option_Constructor_WithNegativePrice_ShouldThrowInvalidEntityFieldException()
    {
        var act = () => new Option("Projector", -10m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(Option.Price));
    }

    [Fact]
    public void Option_Update_WithValidData_ShouldChangeProperties()
    {
        var option = new Option("Projector", 50m);

        option.Update("Screen", 75m);

        option.Name.Should().Be("Screen");
        option.Price.Should().Be(75m);
    }

    [Fact]
    public void Option_Update_WithEmptyName_ShouldThrowInvalidEntityFieldException()
    {
        var option = new Option("Projector", 50m);

        var act = () => option.Update("", 50m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(Option.Name));
    }

    [Fact]
    public void Option_Update_WithNegativePrice_ShouldThrowInvalidEntityFieldException()
    {
        var option = new Option("Projector", 50m);

        var act = () => option.Update("Projector", -10m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(Option.Price));
    }

    #endregion

    #region BookingOption

    [Fact]
    public void BookingOption_Constructor_WithValidData_ShouldSetOptionIdAndPriceAtBooking()
    {
        var optionId = Guid.NewGuid();

        var bookingOption = new BookingOption(optionId, 50m);

        bookingOption.OptionId.Should().Be(optionId);
        bookingOption.PriceAtBooking.Should().Be(50m);
    }

    [Fact]
    public void BookingOption_Constructor_WithNegativePriceAtBooking_ShouldThrowInvalidEntityFieldException()
    {
        var act = () => new BookingOption(Guid.NewGuid(), -10m);

        act.Should().Throw<InvalidEntityFieldException>()
            .Which.FieldName.Should().Be(nameof(BookingOption.PriceAtBooking));
    }

    #endregion
}
