using ConferenceHallBooking.Application.DTOs.Halls;
using ConferenceHallBooking.Application.Features.Halls.Commands;
using ConferenceHallBooking.Application.Features.Halls.Handlers;
using ConferenceHallBooking.Application.Features.Halls.Queries;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace ConferenceHallBooking.UnitTests.Application.Features.Halls;

public class HallHandlerTests
{
    private readonly IHallRepository _hallRepository = Substitute.For<IHallRepository>();
    private readonly IOptionRepository _optionRepository = Substitute.For<IOptionRepository>();
    private readonly IBookingRepository _bookingRepository = Substitute.For<IBookingRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private static void SetHallOptionNavigation(HallOption hallOption, Option option)
    {
        typeof(HallOption)
            .GetProperty(nameof(HallOption.Option))!
            .SetValue(hallOption, option);
    }

    #region CreateHallCommandHandler

    [Fact]
    public async Task CreateHallCommandHandler_WithoutOptions_ShouldCreateHallAndReturnResponse()
    {
        var handler = new CreateHallCommandHandler(_hallRepository, _optionRepository, _unitOfWork);
        var command = new CreateHallCommand("Grand Hall", 100, 250m, []);

        _hallRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new Hall(command.Name, command.Capacity, command.BaseHourlyRate));

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Grand Hall");
        result.Capacity.Should().Be(100);
        result.BaseHourlyRate.Should().Be(250m);
        result.Options.Should().BeEmpty();

        await _hallRepository.Received(1).AddAsync(
            Arg.Is<Hall>(h =>
                h.Name == command.Name &&
                h.Capacity == command.Capacity &&
                h.BaseHourlyRate == command.BaseHourlyRate &&
                h.HallOptions.Count == 0),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateHallCommandHandler_WithExistingOptionIds_ShouldAddOptionsToHall()
    {
        var handler = new CreateHallCommandHandler(_hallRepository, _optionRepository, _unitOfWork);
        var optionId1 = Guid.NewGuid();
        var optionId2 = Guid.NewGuid();
        var existingOption1 = new Option("Projector", 50m);
        var existingOption2 = new Option("Wi-Fi", 30m);

        typeof(Option).GetProperty(nameof(Option.Id))!.SetValue(existingOption1, optionId1);
        typeof(Option).GetProperty(nameof(Option.Id))!.SetValue(existingOption2, optionId2);

        var command = new CreateHallCommand("Grand Hall", 100, 250m, [optionId1, optionId2]);

        _optionRepository.GetByIdsAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(optionId1) && ids.Contains(optionId2)),
                Arg.Any<CancellationToken>())
            .Returns(new List<Option> { existingOption1, existingOption2 });

        var createdHall = new Hall(command.Name, command.Capacity, command.BaseHourlyRate);
        createdHall.AddOption(optionId1);
        createdHall.AddOption(optionId2);

        SetHallOptionNavigation(createdHall.HallOptions.First(ho => ho.OptionId == optionId1), existingOption1);
        SetHallOptionNavigation(createdHall.HallOptions.First(ho => ho.OptionId == optionId2), existingOption2);

        _hallRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(createdHall);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Grand Hall");
        result.Options.Should().HaveCount(2);

        await _optionRepository.Received(1).GetByIdsAsync(
            Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(optionId1) && ids.Contains(optionId2)),
            Arg.Any<CancellationToken>());

        await _hallRepository.Received(1).AddAsync(
            Arg.Is<Hall>(h => h.HallOptions.Count == 2),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateHallCommandHandler_WhenOptionIdsIsNull_ShouldCreateHallWithoutOptions()
    {
        var handler = new CreateHallCommandHandler(_hallRepository, _optionRepository, _unitOfWork);
        var command = new CreateHallCommand("Grand Hall", 100, 250m, null);

        _hallRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new Hall(command.Name, command.Capacity, command.BaseHourlyRate));

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Options.Should().BeEmpty();

        await _optionRepository.DidNotReceive().GetByIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateHallCommandHandler_WhenOptionIdsDoNotExist_ShouldThrowOptionsNotFoundException()
    {
        var handler = new CreateHallCommandHandler(_hallRepository, _optionRepository, _unitOfWork);
        var optionId = Guid.NewGuid();
        var command = new CreateHallCommand("Grand Hall", 100, 250m, [optionId]);

        _optionRepository.GetByIdsAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(optionId)),
                Arg.Any<CancellationToken>())
            .Returns(new List<Option>());

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<OptionsNotFoundException>();

        await _hallRepository.DidNotReceive().AddAsync(Arg.Any<Hall>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateHallCommandHandler_WithDuplicateOptionIds_ShouldDeduplicateBeforeAdding()
    {
        var handler = new CreateHallCommandHandler(_hallRepository, _optionRepository, _unitOfWork);
        var optionId = Guid.NewGuid();
        var existingOption = new Option("Projector", 50m);
        typeof(Option).GetProperty(nameof(Option.Id))!.SetValue(existingOption, optionId);

        var command = new CreateHallCommand("Grand Hall", 100, 250m, [optionId, optionId]);

        _optionRepository.GetByIdsAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 1 && ids.Contains(optionId)),
                Arg.Any<CancellationToken>())
            .Returns(new List<Option> { existingOption });

        var createdHall = new Hall(command.Name, command.Capacity, command.BaseHourlyRate);
        createdHall.AddOption(optionId);

        SetHallOptionNavigation(createdHall.HallOptions.First(), existingOption);

        _hallRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(createdHall);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Options.Should().HaveCount(1);

        await _optionRepository.Received(1).GetByIdsAsync(
            Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateHallCommandHandler_WhenNameAlreadyExists_ShouldThrowHallNameAlreadyExistsException()
    {
        var handler = new CreateHallCommandHandler(_hallRepository, _optionRepository, _unitOfWork);
        var command = new CreateHallCommand("Grand Hall", 100, 250m, []);

        _hallRepository.ExistsByNameAsync("Grand Hall", Arg.Any<CancellationToken>())
            .Returns(true);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<HallNameAlreadyExistsException>();

        await _hallRepository.DidNotReceive().AddAsync(Arg.Any<Hall>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region UpdateHallCommandHandler

    [Fact]
    public async Task UpdateHallCommandHandler_WhenHallDoesNotExist_ShouldThrowNotFoundException()
    {
        var handler = new UpdateHallCommandHandler(_hallRepository, _optionRepository, _bookingRepository, _unitOfWork);
        var hallId = Guid.NewGuid();
        var command = new UpdateHallCommand(hallId, "Updated Name", 150, 300m, []);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns((Hall?)null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException<Hall>>();

        _hallRepository.DidNotReceive().Update(Arg.Any<Hall>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateHallCommandHandler_WhenValid_ShouldUpdateHallPropertiesAndReturnResponse()
    {
        var handler = new UpdateHallCommandHandler(_hallRepository, _optionRepository, _bookingRepository, _unitOfWork);
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("Old Name", 50, 100m);
        var command = new UpdateHallCommand(hallId, "New Name", 80, 150m, []);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("New Name");
        result.Capacity.Should().Be(80);
        result.BaseHourlyRate.Should().Be(150m);

        existingHall.Name.Should().Be("New Name");
        existingHall.Capacity.Should().Be(80);
        existingHall.BaseHourlyRate.Should().Be(150m);

        _hallRepository.Received(1).Update(existingHall);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateHallCommandHandler_WithExistingOptionIds_ShouldAddThemToHall()
    {
        var handler = new UpdateHallCommandHandler(_hallRepository, _optionRepository, _bookingRepository, _unitOfWork);
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("Existing Hall", 50, 100m);
        var optionId = Guid.NewGuid();
        var existingOption = new Option("Projector", 40m);
        typeof(Option).GetProperty(nameof(Option.Id))!.SetValue(existingOption, optionId);

        var command = new UpdateHallCommand(hallId, "Updated Hall", 60, 120m, [optionId]);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);

        _optionRepository.GetByIdsAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(optionId)),
                Arg.Any<CancellationToken>())
            .Returns(new List<Option> { existingOption });

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Hall");
        result.Capacity.Should().Be(60);
        result.BaseHourlyRate.Should().Be(120m);

        existingHall.HallOptions.Should().HaveCount(1);
        existingHall.HallOptions.First().OptionId.Should().Be(optionId);

        _hallRepository.Received(1).Update(existingHall);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateHallCommandHandler_WhenOptionIdsIsEmpty_ShouldRemoveAllExistingOptions()
    {
        var handler = new UpdateHallCommandHandler(_hallRepository, _optionRepository, _bookingRepository, _unitOfWork);
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("Existing Hall", 50, 100m);
        existingHall.AddOption(Guid.NewGuid());
        existingHall.AddOption(Guid.NewGuid());

        var command = new UpdateHallCommand(hallId, "Updated Hall", 60, 120m, []);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Options.Should().BeEmpty();
        existingHall.HallOptions.Should().BeEmpty();

        await _optionRepository.DidNotReceive().GetByIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(),
            Arg.Any<CancellationToken>());

        _hallRepository.Received(1).Update(existingHall);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateHallCommandHandler_WhenReplacingSomeOptions_ShouldRemoveOldAndAddNew()
    {
        var handler = new UpdateHallCommandHandler(_hallRepository, _optionRepository, _bookingRepository, _unitOfWork);
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("Existing Hall", 50, 100m);
        var keepOptionId = Guid.NewGuid();
        var removeOptionId = Guid.NewGuid();
        var addOptionId = Guid.NewGuid();
        var keepOption = new Option("Keep Option", 30m);
        var addOption = new Option("Add Option", 40m);
        typeof(Option).GetProperty(nameof(Option.Id))!.SetValue(keepOption, keepOptionId);
        typeof(Option).GetProperty(nameof(Option.Id))!.SetValue(addOption, addOptionId);

        existingHall.AddOption(keepOptionId);
        existingHall.AddOption(removeOptionId);

        var hallAfterSync = new Hall("Existing Hall", 50, 100m);
        hallAfterSync.Update("Updated Hall", 60, 120m);
        hallAfterSync.AddOption(keepOptionId);
        hallAfterSync.AddOption(addOptionId);
        SetHallOptionNavigation(hallAfterSync.HallOptions.First(ho => ho.OptionId == keepOptionId), keepOption);
        SetHallOptionNavigation(hallAfterSync.HallOptions.First(ho => ho.OptionId == addOptionId), addOption);

        var command = new UpdateHallCommand(hallId, "Updated Hall", 60, 120m, [keepOptionId, addOptionId]);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall, hallAfterSync);

        _optionRepository.GetByIdsAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(keepOptionId) && ids.Contains(addOptionId)),
                Arg.Any<CancellationToken>())
            .Returns(new List<Option> { keepOption, addOption });

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Options.Should().HaveCount(2);

        existingHall.HallOptions.Should().HaveCount(2);
        existingHall.HallOptions.Select(ho => ho.OptionId).Should().Contain([keepOptionId, addOptionId]);
        existingHall.HallOptions.Select(ho => ho.OptionId).Should().NotContain(removeOptionId);

        _hallRepository.Received(1).Update(existingHall);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateHallCommandHandler_WhenOptionIdsDoNotExist_ShouldThrowOptionsNotFoundException()
    {
        var handler = new UpdateHallCommandHandler(_hallRepository, _optionRepository, _bookingRepository, _unitOfWork);
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("Existing Hall", 50, 100m);
        var nonexistentOptionId = Guid.NewGuid();

        var command = new UpdateHallCommand(hallId, "Updated Hall", 60, 120m, [nonexistentOptionId]);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);

        _optionRepository.GetByIdsAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(nonexistentOptionId)),
                Arg.Any<CancellationToken>())
            .Returns(new List<Option>());

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<OptionsNotFoundException>();

        _hallRepository.DidNotReceive().Update(Arg.Any<Hall>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateHallCommandHandler_WhenNewNameAlreadyExists_ShouldThrowHallNameAlreadyExistsException()
    {
        var handler = new UpdateHallCommandHandler(_hallRepository, _optionRepository, _bookingRepository, _unitOfWork);
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("Old Name", 50, 100m);
        var command = new UpdateHallCommand(hallId, "Existing Hall", 50, 100m, []);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);

        _hallRepository.ExistsByNameAsync("Existing Hall", Arg.Any<CancellationToken>())
            .Returns(true);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<HallNameAlreadyExistsException>();

        _hallRepository.DidNotReceive().Update(Arg.Any<Hall>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region DeleteHallCommandHandler

    [Fact]
    public async Task DeleteHallCommandHandler_WhenHallExists_ShouldDeleteAndSaveChanges()
    {
        var handler = new DeleteHallCommandHandler(_hallRepository, _bookingRepository, _unitOfWork);
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("To Delete", 20, 50m);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);
        _bookingRepository.GetBookingCountByHallIdAsync(existingHall.Id, Arg.Any<CancellationToken>())
            .Returns(0);

        await handler.Handle(new DeleteHallCommand(hallId), CancellationToken.None);

        _hallRepository.Received(1).Delete(existingHall);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteHallCommandHandler_WhenHallDoesNotExist_ShouldThrowNotFoundException()
    {
        var handler = new DeleteHallCommandHandler(_hallRepository, _bookingRepository, _unitOfWork);
        var hallId = Guid.NewGuid();

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns((Hall?)null);

        var act = () => handler.Handle(new DeleteHallCommand(hallId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException<Hall>>();

        _hallRepository.DidNotReceive().Delete(Arg.Any<Hall>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteHallCommandHandler_WhenHallHasBookings_ShouldThrowHallHasBookingsException()
    {
        var handler = new DeleteHallCommandHandler(_hallRepository, _bookingRepository, _unitOfWork);
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("Has Bookings", 20, 50m);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);
        _bookingRepository.GetBookingCountByHallIdAsync(existingHall.Id, Arg.Any<CancellationToken>())
            .Returns(3);

        var act = () => handler.Handle(new DeleteHallCommand(hallId), CancellationToken.None);

        var exception = await act.Should().ThrowAsync<HallHasBookingsException>();
        exception.Which.HallId.Should().Be(existingHall.Id);
        exception.Which.BookingCount.Should().Be(3);

        _hallRepository.DidNotReceive().Delete(Arg.Any<Hall>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region GetHallByIdQueryHandler

    [Fact]
    public async Task GetHallByIdQueryHandler_WhenHallExists_ShouldReturnHallResponse()
    {
        var handler = new GetHallByIdQueryHandler(_hallRepository);
        var hallId = Guid.NewGuid();
        var hall = new Hall("Grand Hall", 100, 250m);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(hall);

        var result = await handler.Handle(new GetHallByIdQuery(hallId), CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(hall.Id);
        result.Name.Should().Be("Grand Hall");
        result.Capacity.Should().Be(100);
        result.BaseHourlyRate.Should().Be(250m);
        result.Options.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHallByIdQueryHandler_WhenHallDoesNotExist_ShouldThrowNotFoundException()
    {
        var handler = new GetHallByIdQueryHandler(_hallRepository);
        var hallId = Guid.NewGuid();

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns((Hall?)null);

        var act = () => handler.Handle(new GetHallByIdQuery(hallId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException<Hall>>();
    }

    [Fact]
    public async Task GetHallByIdQueryHandler_WhenHallHasOptions_ShouldMapOptionsCorrectly()
    {
        var handler = new GetHallByIdQueryHandler(_hallRepository);
        var hallId = Guid.NewGuid();
        var hall = new Hall("Grand Hall", 100, 250m);
        var option = new Option("Projector", 50m);

        hall.AddOption(option.Id);
        SetHallOptionNavigation(hall.HallOptions.First(), option);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(hall);

        var result = await handler.Handle(new GetHallByIdQuery(hallId), CancellationToken.None);

        result.Options.Should().HaveCount(1);
        var opt = result.Options.First();
        opt.Id.Should().Be(option.Id);
        opt.Name.Should().Be("Projector");
        opt.Price.Should().Be(50m);
    }

    #endregion

    #region SearchAvailableHallsQueryHandler

    [Fact]
    public async Task SearchAvailableHallsQueryHandler_WhenHallsExist_ShouldReturnMappedResponses()
    {
        var handler = new SearchAvailableHallsQueryHandler(_hallRepository);
        var start = DateTimeOffset.UtcNow.AddDays(1);
        var end = start.AddHours(3);
        var query = new SearchAvailableHallsQuery(start, end, 50);

        var hall = new Hall("Available Hall", 100, 200m);

        _hallRepository.GetAvailableHallsAsync(start, end, 50, Arg.Any<CancellationToken>())
            .Returns([hall]);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        var response = result.First();
        response.Id.Should().Be(hall.Id);
        response.Name.Should().Be("Available Hall");
        response.Capacity.Should().Be(100);
        response.BaseHourlyRate.Should().Be(200m);
        response.Options.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAvailableHallsQueryHandler_WhenNoHallsAvailable_ShouldReturnEmptyCollection()
    {
        var handler = new SearchAvailableHallsQueryHandler(_hallRepository);
        var start = DateTimeOffset.UtcNow.AddDays(1);
        var end = start.AddHours(3);
        var query = new SearchAvailableHallsQuery(start, end, 50);

        _hallRepository.GetAvailableHallsAsync(start, end, 50, Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    #endregion
}
