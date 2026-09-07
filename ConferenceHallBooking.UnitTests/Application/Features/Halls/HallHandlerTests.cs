using ConferenceHallBooking.Application.DTOs.Halls;
using ConferenceHallBooking.Application.Features.Halls.Commands;
using ConferenceHallBooking.Application.Features.Halls.Handlers;
using ConferenceHallBooking.Application.Features.Halls.Queries;
using ConferenceHallBooking.Application.Interfaces.Halls;
using FluentAssertions;
using NSubstitute;

namespace ConferenceHallBooking.UnitTests.Application.Features.Halls;

public class HallHandlerTests
{
    private readonly IHallService _hallService = Substitute.For<IHallService>();

    #region CreateHallCommandHandler

    [Fact]
    public async Task CreateHallCommandHandler_WhenCalled_ShouldDelegateToHallService()
    {
        var handler = new CreateHallCommandHandler(_hallService);
        var command = new CreateHallCommand("Grand Hall", 100, 250m, []);
        var expectedId = Guid.NewGuid();

        _hallService.CreateAsync(Arg.Any<CreateHallRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().Be(expectedId);
        await _hallService.Received(1).CreateAsync(
            Arg.Is<CreateHallRequest>(r =>
                r.Name == "Grand Hall" &&
                r.Capacity == 100 &&
                r.BaseHourlyRate == 250m),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateHallCommandHandler_WithOptions_ShouldPassOptionsToService()
    {
        var handler = new CreateHallCommandHandler(_hallService);
        var optionIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var command = new CreateHallCommand("Grand Hall", 100, 250m, optionIds);

        _hallService.CreateAsync(Arg.Any<CreateHallRequest>(), Arg.Any<CancellationToken>())
            .Returns(Guid.NewGuid());

        await handler.Handle(command, CancellationToken.None);

        await _hallService.Received(1).CreateAsync(
            Arg.Is<CreateHallRequest>(r => r.OptionIds == optionIds),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region UpdateHallCommandHandler

    [Fact]
    public async Task UpdateHallCommandHandler_WhenCalled_ShouldDelegateToHallService()
    {
        var handler = new UpdateHallCommandHandler(_hallService);
        var hallId = Guid.NewGuid();
        var command = new UpdateHallCommand(hallId, "Updated Hall", 80, 150m, []);

        await handler.Handle(command, CancellationToken.None);

        await _hallService.Received(1).UpdateAsync(
            hallId,
            Arg.Is<UpdateHallRequest>(r =>
                r.Name == "Updated Hall" &&
                r.Capacity == 80 &&
                r.BaseHourlyRate == 150m),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region DeleteHallCommandHandler

    [Fact]
    public async Task DeleteHallCommandHandler_WhenCalled_ShouldDelegateToHallService()
    {
        var handler = new DeleteHallCommandHandler(_hallService);
        var hallId = Guid.NewGuid();
        var command = new DeleteHallCommand(hallId);

        await handler.Handle(command, CancellationToken.None);

        await _hallService.Received(1).DeleteAsync(hallId, Arg.Any<CancellationToken>());
    }

    #endregion

    #region SearchAvailableHallsQueryHandler

    [Fact]
    public async Task SearchAvailableHallsQueryHandler_WhenCalled_ShouldDelegateToHallService()
    {
        var handler = new SearchAvailableHallsQueryHandler(_hallService);
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var endTime = startTime.AddHours(3);
        var query = new SearchAvailableHallsQuery(startTime, endTime, 50);

        var expectedHalls = new List<HallResponse>
        {
            new(Guid.NewGuid(), "Available Hall", 100, 200m, [])
        };

        _hallService.SearchAvailableAsync(Arg.Any<SearchAvailableHallsRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedHalls);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Available Hall");

        await _hallService.Received(1).SearchAvailableAsync(
            Arg.Is<SearchAvailableHallsRequest>(r =>
                r.StartTime == startTime &&
                r.EndTime == endTime &&
                r.Capacity == 50),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchAvailableHallsQueryHandler_WhenNoHalls_ShouldReturnEmptyCollection()
    {
        var handler = new SearchAvailableHallsQueryHandler(_hallService);
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var endTime = startTime.AddHours(3);
        var query = new SearchAvailableHallsQuery(startTime, endTime, 50);

        _hallService.SearchAvailableAsync(Arg.Any<SearchAvailableHallsRequest>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    #endregion
}
