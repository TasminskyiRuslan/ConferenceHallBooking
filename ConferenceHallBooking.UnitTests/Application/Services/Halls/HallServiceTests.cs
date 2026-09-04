namespace ConferenceHallBooking.UnitTests.Application.Services.Halls;

using ConferenceHallBooking.Application.DTOs.Halls;
using ConferenceHallBooking.Application.Exceptions;
using ConferenceHallBooking.Application.Services.Halls;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

public class HallServiceTests
{
    private readonly IHallRepository _hallRepository = Substitute.For<IHallRepository>();
    private readonly IOptionRepository _optionRepository = Substitute.For<IOptionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ILogger<HallService> _logger = Substitute.For<ILogger<HallService>>();

    private readonly HallService _service;

    public HallServiceTests()
    {
        _service = new HallService(_hallRepository, _optionRepository, _unitOfWork, _logger);
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithoutOptions_ShouldCreateHallAndSaveChanges()
    {
        // Arrange
        var request = new CreateHallRequest("Grand Hall", 100, 250m, new List<Guid>());

        // Act
        var hallId = await _service.CreateAsync(request);

        // Assert
        hallId.Should().NotBeEmpty();

        await _hallRepository.Received(1).AddAsync(
            Arg.Is<Hall>(h => h.Name == request.Name && h.Capacity == request.Capacity && h.BaseHourlyRate == request.BaseHourlyRate),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithValidOptions_ShouldValidateOptionsAndAddThemToHall()
    {
        // Arrange
        var optionId1 = Guid.NewGuid();
        var optionId2 = Guid.NewGuid();
        var optionIds = new List<Guid> { optionId1, optionId2, optionId1 };
        var request = new CreateHallRequest("Grand Hall", 100, 250m, optionIds);

        var existingOptions = new List<Option>
        {
            new("Projector", 50m),
            new("Wi-Fi", 30m)
        };

        _optionRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(existingOptions);

        // Act
        var hallId = await _service.CreateAsync(request);

        // Assert
        hallId.Should().NotBeEmpty();

        await _optionRepository.Received(1).GetByIdsAsync(
            Arg.Is<IEnumerable<Guid>>(ids => ids.Count() == 2),
            Arg.Any<CancellationToken>());

        await _hallRepository.Received(1).AddAsync(
            Arg.Is<Hall>(h => h.HallOptions.Count == 2),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WhenSomeOptionsDoNotExist_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var optionId1 = Guid.NewGuid();
        var optionId2 = Guid.NewGuid();
        var request = new CreateHallRequest("Grand Hall", 100, 250m, new List<Guid> { optionId1, optionId2 });

        _optionRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Option> { new("Projector", 50m) });

        // Act
        var act = () => _service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("One or more specified options do not exist.");

        await _hallRepository.DidNotReceive().AddAsync(Arg.Any<Hall>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WhenOptionIdsIsNull_ShouldCreateHallWithoutOptions()
    {
        // Arrange
        var request = new CreateHallRequest("Grand Hall", 100, 250m, null!);

        // Act
        var hallId = await _service.CreateAsync(request);

        // Assert
        hallId.Should().NotBeEmpty();

        await _optionRepository.DidNotReceive().GetByIdsAsync(
            Arg.Any<IEnumerable<Guid>>(),
            Arg.Any<CancellationToken>());

        await _hallRepository.Received(1).AddAsync(
            Arg.Is<Hall>(h => h.HallOptions.Count == 0),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WhenHallDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var hallId = Guid.NewGuid();
        var request = new UpdateHallRequest("Updated Name", 150, 300m, new List<Guid>());

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns((Hall?)null);

        // Act
        var act = () => _service.UpdateAsync(hallId, request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Conference hall with ID '{hallId}' was not found.");

        _hallRepository.DidNotReceive().Update(Arg.Any<Hall>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WhenSomeOptionsDoNotExist_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("Existing Hall", 50, 100m);
        var invalidOptionId = Guid.NewGuid();

        var request = new UpdateHallRequest("Updated Hall", 60, 120m, new List<Guid> { invalidOptionId });

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);

        _optionRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Option>());

        // Act
        var act = () => _service.UpdateAsync(hallId, request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("One or more specified options do not exist.");

        _hallRepository.DidNotReceive().Update(Arg.Any<Hall>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WhenValid_ShouldUpdateHallPropertiesAndSynchronizeOptions()
    {
        // Arrange
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("Old Name", 50, 100m);

        var oldOptionId = Guid.NewGuid();
        var newOptionId = Guid.NewGuid();

        existingHall.AddOption(oldOptionId);

        var request = new UpdateHallRequest("New Name", 80, 150m, new List<Guid> { newOptionId });

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);

        _optionRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Option> { new("New Option", 40m) });

        // Act
        await _service.UpdateAsync(hallId, request);

        // Assert
        existingHall.Name.Should().Be("New Name");
        existingHall.Capacity.Should().Be(80);
        existingHall.BaseHourlyRate.Should().Be(150m);

        existingHall.HallOptions.Should().HaveCount(1);
        existingHall.HallOptions.First().OptionId.Should().Be(newOptionId);

        _hallRepository.Received(1).Update(existingHall);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WhenOptionIdsIsEmpty_ShouldRemoveAllExistingOptionsWithoutCallingOptionRepo()
    {
        // Arrange
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("Existing Hall", 50, 100m);
        existingHall.AddOption(Guid.NewGuid());

        var request = new UpdateHallRequest("Updated Hall", 60, 120m, new List<Guid>());

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);

        // Act
        await _service.UpdateAsync(hallId, request);

        // Assert
        existingHall.HallOptions.Should().BeEmpty();

        await _optionRepository.DidNotReceive().GetByIdsAsync(
            Arg.Any<IEnumerable<Guid>>(),
            Arg.Any<CancellationToken>());

        _hallRepository.Received(1).Update(existingHall);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WhenOptionIdsIsNull_ShouldRemoveAllExistingOptionsWithoutThrowing()
    {
        // Arrange
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("Existing Hall", 50, 100m);
        existingHall.AddOption(Guid.NewGuid());

        var request = new UpdateHallRequest("Updated Hall", 60, 120m, null!);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);

        // Act
        await _service.UpdateAsync(hallId, request);

        // Assert
        existingHall.HallOptions.Should().BeEmpty();

        await _optionRepository.DidNotReceive().GetByIdsAsync(
            Arg.Any<IEnumerable<Guid>>(),
            Arg.Any<CancellationToken>());

        _hallRepository.Received(1).Update(existingHall);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenHallExists_ShouldDeleteAndSaveChanges()
    {
        // Arrange
        var hallId = Guid.NewGuid();
        var existingHall = new Hall("To Delete", 20, 50m);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(existingHall);

        // Act
        await _service.DeleteAsync(hallId);

        // Assert
        _hallRepository.Received(1).Delete(existingHall);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_WhenHallDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var hallId = Guid.NewGuid();

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns((Hall?)null);

        // Act
        var act = () => _service.DeleteAsync(hallId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Conference hall with ID '{hallId}' was not found.");

        _hallRepository.DidNotReceive().Delete(Arg.Any<Hall>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region SearchAvailableAsync Tests

    [Fact]
    public async Task SearchAvailableAsync_ShouldReturnMappedHallResponses()
    {
        // Arrange
        var start = DateTimeOffset.UtcNow.AddDays(1);
        var end = start.AddHours(3);
        var request = new SearchAvailableHallsRequest(start, end, 50);

        var hall = new Hall("Available Hall", 100, 200m);
        var availableHalls = new List<Hall> { hall };

        _hallRepository.GetAvailableHallsAsync(start, end, 50, Arg.Any<CancellationToken>())
            .Returns(availableHalls);

        // Act
        var result = await _service.SearchAvailableAsync(request);

        // Assert
        result.Should().HaveCount(1);
        var response = result.First();
        response.Id.Should().Be(hall.Id);
        response.Name.Should().Be("Available Hall");
        response.Capacity.Should().Be(100);
        response.BaseHourlyRate.Should().Be(200m);

        await _hallRepository.Received(1).GetAvailableHallsAsync(start, end, 50, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchAvailableAsync_WhenHallHasOptions_ShouldMapOptionsCorrectly()
    {
        // Arrange
        var start = DateTimeOffset.UtcNow.AddDays(1);
        var end = start.AddHours(3);
        var request = new SearchAvailableHallsRequest(start, end, 50);

        var hall = new Hall("Available Hall", 100, 200m);
        var option = new Option("Projector", 50m);

        hall.AddOption(option.Id);

        typeof(HallOption)
            .GetProperty(nameof(HallOption.Option))?
            .SetValue(hall.HallOptions.First(), option);

        _hallRepository.GetAvailableHallsAsync(start, end, 50, Arg.Any<CancellationToken>())
            .Returns(new List<Hall> { hall });

        // Act
        var result = await _service.SearchAvailableAsync(request);

        // Assert
        result.First().Options.Should().HaveCount(1);
    }

    #endregion
}