using ConferenceHallBooking.Application.DTOs.Halls;
using ConferenceHallBooking.Application.Services.Halls;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace ConferenceHallBooking.UnitTests.Application.Services.Halls;

public class HallServiceTests
{
    private readonly IHallRepository _hallRepository = Substitute.For<IHallRepository>();
    private readonly IOptionRepository _optionRepository = Substitute.For<IOptionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly HallService _service;

    public HallServiceTests()
    {
        _service = new HallService(_hallRepository, _optionRepository, _unitOfWork);
    }

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_WithoutOptions_ShouldCreateHallAndSaveChanges()
    {
        var request = new CreateHallRequest("Grand Hall", 100, 250m, []);

        var hallId = await _service.CreateAsync(request);

        hallId.Should().NotBeEmpty();

        await _hallRepository.Received(1).AddAsync(
            Arg.Is<Hall>(h =>
                h.Name == request.Name &&
                h.Capacity == request.Capacity &&
                h.BaseHourlyRate == request.BaseHourlyRate &&
                h.HallOptions.Count == 0),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithValidOptions_ShouldValidateAndAddThemToHall()
    {
        var optionId1 = Guid.NewGuid();
        var optionId2 = Guid.NewGuid();
        var request = new CreateHallRequest("Grand Hall", 100, 250m, [optionId1, optionId2, optionId1]);

        var existingOptions = new List<Option>
        {
            new("Projector", 50m),
            new("Wi-Fi", 30m)
        };

        _optionRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(existingOptions);

        var hallId = await _service.CreateAsync(request);

        hallId.Should().NotBeEmpty();

        await _optionRepository.Received(1).GetByIdsAsync(
            Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 2),
            Arg.Any<CancellationToken>());

        await _hallRepository.Received(1).AddAsync(
            Arg.Is<Hall>(h => h.HallOptions.Count == 2),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WhenOptionIdsIsNull_ShouldCreateHallWithoutOptions()
    {
        var request = new CreateHallRequest("Grand Hall", 100, 250m, null!);

        var hallId = await _service.CreateAsync(request);

        hallId.Should().NotBeEmpty();

        await _optionRepository.DidNotReceive().GetByIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(),
            Arg.Any<CancellationToken>());

        await _hallRepository.Received(1).AddAsync(
            Arg.Is<Hall>(h => h.HallOptions.Count == 0),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WhenSomeOptionsDoNotExist_ShouldThrowOptionsNotFoundException()
    {
        var optionId1 = Guid.NewGuid();
        var optionId2 = Guid.NewGuid();
        var request = new CreateHallRequest("Grand Hall", 100, 250m, [optionId1, optionId2]);

        _optionRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([new Option("Projector", 50m)]);

        var act = () => _service.CreateAsync(request);

        await act.Should().ThrowAsync<OptionsNotFoundException>();

        await _hallRepository.DidNotReceive().AddAsync(Arg.Any<Hall>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateOptionIds_ShouldDeduplicateBeforeValidation()
    {
        var optionId = Guid.NewGuid();
        var request = new CreateHallRequest("Grand Hall", 100, 250m, [optionId, optionId, optionId]);

        _optionRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([new Option("Projector", 50m)]);

        var hallId = await _service.CreateAsync(request);

        hallId.Should().NotBeEmpty();

        await _optionRepository.Received(1).GetByIdsAsync(
            Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 1),
            Arg.Any<CancellationToken>());

        await _hallRepository.Received(1).AddAsync(
            Arg.Is<Hall>(h => h.HallOptions.Count == 1),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_WhenHallDoesNotExist_ShouldThrowNotFoundException()
    {
        var hallId = Guid.NewGuid();
        var request = new UpdateHallRequest("Updated Name", 150, 300m, []);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns((Hall?)null);

        var act = () => _service.UpdateAsync(hallId, request);

        await act.Should().ThrowAsync<NotFoundException<Hall>>();

        _hallRepository.DidNotReceive().Update(Arg.Any<Hall>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WhenValid_ShouldUpdateHallPropertiesAndSaveChanges()
    {
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("Old Name", 50, 100m);

        var request = new UpdateHallRequest("New Name", 80, 150m, []);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);

        await _service.UpdateAsync(hallId, request);

        existingHall.Name.Should().Be("New Name");
        existingHall.Capacity.Should().Be(80);
        existingHall.BaseHourlyRate.Should().Be(150m);

        _hallRepository.Received(1).Update(existingHall);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WithNewOptions_ShouldAddThemToHall()
    {
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("Existing Hall", 50, 100m);
        var newOptionId = Guid.NewGuid();

        var request = new UpdateHallRequest("Updated Hall", 60, 120m, [newOptionId]);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);

        _optionRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([new Option("New Option", 40m)]);

        await _service.UpdateAsync(hallId, request);

        existingHall.HallOptions.Should().HaveCount(1);
        existingHall.HallOptions.First().OptionId.Should().Be(newOptionId);

        _hallRepository.Received(1).Update(existingHall);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WhenOptionIdsIsEmpty_ShouldRemoveAllExistingOptions()
    {
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("Existing Hall", 50, 100m);
        existingHall.AddOption(Guid.NewGuid());
        existingHall.AddOption(Guid.NewGuid());

        var request = new UpdateHallRequest("Updated Hall", 60, 120m, []);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);

        await _service.UpdateAsync(hallId, request);

        existingHall.HallOptions.Should().BeEmpty();

        await _optionRepository.DidNotReceive().GetByIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(),
            Arg.Any<CancellationToken>());

        _hallRepository.Received(1).Update(existingHall);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WhenSomeOptionsDoNotExist_ShouldThrowOptionsNotFoundException()
    {
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("Existing Hall", 50, 100m);
        var invalidOptionId = Guid.NewGuid();

        var request = new UpdateHallRequest("Updated Hall", 60, 120m, [invalidOptionId]);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);

        _optionRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var act = () => _service.UpdateAsync(hallId, request);

        await act.Should().ThrowAsync<OptionsNotFoundException>();

        _hallRepository.DidNotReceive().Update(Arg.Any<Hall>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WhenKeepingSameOptions_ShouldNotAddOrRemoveAnything()
    {
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("Existing Hall", 50, 100m);
        var optionId = Guid.NewGuid();
        existingHall.AddOption(optionId);

        var request = new UpdateHallRequest("Updated Hall", 60, 120m, [optionId]);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);

        _optionRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([new Option("Existing Option", 30m)]);

        await _service.UpdateAsync(hallId, request);

        existingHall.HallOptions.Should().HaveCount(1);
        existingHall.HallOptions.First().OptionId.Should().Be(optionId);

        _hallRepository.Received(1).Update(existingHall);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WhenReplacingSomeOptions_ShouldRemoveOldAndAddNew()
    {
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("Existing Hall", 50, 100m);
        var keepOptionId = Guid.NewGuid();
        var removeOptionId = Guid.NewGuid();
        var addOptionId = Guid.NewGuid();

        existingHall.AddOption(keepOptionId);
        existingHall.AddOption(removeOptionId);

        var request = new UpdateHallRequest("Updated Hall", 60, 120m, [keepOptionId, addOptionId]);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);

        _optionRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([new Option("Keep Option", 30m), new Option("Add Option", 40m)]);

        await _service.UpdateAsync(hallId, request);

        existingHall.HallOptions.Should().HaveCount(2);
        existingHall.HallOptions.Select(ho => ho.OptionId).Should().Contain([keepOptionId, addOptionId]);
        existingHall.HallOptions.Select(ho => ho.OptionId).Should().NotContain(removeOptionId);

        _hallRepository.Received(1).Update(existingHall);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_WhenHallExists_ShouldDeleteAndSaveChanges()
    {
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("To Delete", 20, 50m);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);

        await _service.DeleteAsync(hallId);

        _hallRepository.Received(1).Delete(existingHall);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_WhenHallDoesNotExist_ShouldThrowNotFoundException()
    {
        var hallId = Guid.NewGuid();

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns((Hall?)null);

        var act = () => _service.DeleteAsync(hallId);

        await act.Should().ThrowAsync<NotFoundException<Hall>>();

        _hallRepository.DidNotReceive().Delete(Arg.Any<Hall>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region SearchAvailableAsync

    [Fact]
    public async Task SearchAvailableAsync_WhenHallsExist_ShouldReturnMappedResponses()
    {
        var start = DateTimeOffset.UtcNow.AddDays(1);
        var end = start.AddHours(3);
        var request = new SearchAvailableHallsRequest(start, end, 50);

        var hall = new Hall("Available Hall", 100, 200m);

        _hallRepository.GetAvailableHallsAsync(start, end, 50, Arg.Any<CancellationToken>())
            .Returns([hall]);

        var result = await _service.SearchAvailableAsync(request);

        result.Should().HaveCount(1);

        var response = result.First();
        response.Id.Should().Be(hall.Id);
        response.Name.Should().Be("Available Hall");
        response.Capacity.Should().Be(100);
        response.BaseHourlyRate.Should().Be(200m);
        response.Options.Should().BeEmpty();

        await _hallRepository.Received(1).GetAvailableHallsAsync(start, end, 50, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchAvailableAsync_WhenNoHallsAvailable_ShouldReturnEmptyCollection()
    {
        var start = DateTimeOffset.UtcNow.AddDays(1);
        var end = start.AddHours(3);
        var request = new SearchAvailableHallsRequest(start, end, 50);

        _hallRepository.GetAvailableHallsAsync(start, end, 50, Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await _service.SearchAvailableAsync(request);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAvailableAsync_WhenHallHasOptions_ShouldMapOptionsCorrectly()
    {
        var start = DateTimeOffset.UtcNow.AddDays(1);
        var end = start.AddHours(3);
        var request = new SearchAvailableHallsRequest(start, end, 50);

        var hall = new Hall("Available Hall", 100, 200m);
        var option = new Option("Projector", 50m);

        hall.AddOption(option.Id);

        typeof(HallOption)
            .GetProperty(nameof(HallOption.Option))!
            .SetValue(hall.HallOptions.First(), option);

        _hallRepository.GetAvailableHallsAsync(start, end, 50, Arg.Any<CancellationToken>())
            .Returns([hall]);

        var result = await _service.SearchAvailableAsync(request);

        var response = result.First();
        response.Options.Should().HaveCount(1);

        var opt = response.Options.First();
        opt.Id.Should().Be(option.Id);
        opt.Name.Should().Be("Projector");
        opt.Price.Should().Be(50m);
    }

    #endregion
}
