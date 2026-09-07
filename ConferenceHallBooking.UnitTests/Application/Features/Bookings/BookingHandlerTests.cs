using ConferenceHallBooking.Application.DTOs.Bookings;
using ConferenceHallBooking.Application.Features.Bookings.Commands;
using ConferenceHallBooking.Application.Features.Bookings.Handlers;
using ConferenceHallBooking.Application.Interfaces.Bookings;
using FluentAssertions;
using NSubstitute;

namespace ConferenceHallBooking.UnitTests.Application.Features.Bookings;

public class BookingHandlerTests
{
    private readonly IBookingService _bookingService = Substitute.For<IBookingService>();

    [Fact]
    public async Task CreateBookingCommandHandler_WhenCalled_ShouldDelegateToBookingService()
    {
        var handler = new CreateBookingCommandHandler(_bookingService);
        var hallId = Guid.NewGuid();
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var command = new CreateBookingCommand(hallId, startTime, 3m, null);

        var expectedResponse = new BookingResponse(
            Guid.NewGuid(),
            hallId,
            "Conference Room A",
            50,
            100m,
            startTime,
            startTime.AddHours(3),
            3m,
            [],
            300m,
            0m,
            300m);

        _bookingService.CreateAsync(Arg.Any<CreateBookingRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().Be(expectedResponse);
        result.HallId.Should().Be(hallId);
        result.DurationHours.Should().Be(3m);
        result.TotalCost.Should().Be(300m);

        await _bookingService.Received(1).CreateAsync(
            Arg.Is<CreateBookingRequest>(r =>
                r.HallId == hallId &&
                r.StartTime == startTime &&
                r.DurationHours == 3m &&
                r.OptionIds == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateBookingCommandHandler_WithOptions_ShouldPassOptionsToService()
    {
        var handler = new CreateBookingCommandHandler(_bookingService);
        var hallId = Guid.NewGuid();
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var optionIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var command = new CreateBookingCommand(hallId, startTime, 2m, optionIds);

        var expectedResponse = new BookingResponse(
            Guid.NewGuid(),
            hallId,
            "Conference Room A",
            50,
            100m,
            startTime,
            startTime.AddHours(2),
            2m,
            [],
            200m,
            100m,
            300m);

        _bookingService.CreateAsync(Arg.Any<CreateBookingRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().Be(expectedResponse);

        await _bookingService.Received(1).CreateAsync(
            Arg.Is<CreateBookingRequest>(r => r.OptionIds == optionIds),
            Arg.Any<CancellationToken>());
    }
}
