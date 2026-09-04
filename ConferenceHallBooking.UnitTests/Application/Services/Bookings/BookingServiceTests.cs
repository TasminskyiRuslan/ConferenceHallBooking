namespace ConferenceHallBooking.UnitTests.Application.Services.Bookings;

using ConferenceHallBooking.Application.DTOs.Bookings;
using ConferenceHallBooking.Application.Exceptions;
using ConferenceHallBooking.Application.Services.Bookings;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Interfaces;
using ConferenceHallBookingApi.ConferenceHallBooking.Application.Interfaces.Bookings;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

public class BookingServiceTests
{
    private readonly IBookingRepository _bookingRepository = Substitute.For<IBookingRepository>();
    private readonly IHallRepository _hallRepository = Substitute.For<IHallRepository>();
    private readonly IOptionRepository _optionRepository = Substitute.For<IOptionRepository>();
    private readonly IPricingService _pricingService = Substitute.For<IPricingService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ILogger<BookingService> _logger = Substitute.For<ILogger<BookingService>>();

    private readonly BookingService _service;

    public BookingServiceTests()
    {
        _service = new BookingService(
            _bookingRepository,
            _hallRepository,
            _optionRepository,
            _pricingService,
            _unitOfWork,
            _logger);
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WhenHallDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var hallId = Guid.NewGuid();
        var request = new CreateBookingRequest(hallId, DateTimeOffset.UtcNow.AddDays(1), 2m, null);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns((Hall?)null);

        // Act
        var act = () => _service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Conference hall with ID '{hallId}' was not found.");

        await _bookingRepository.DidNotReceive().HasOverlappingBookingAsync(
            Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());

        await _bookingRepository.DidNotReceive().AddAsync(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WhenTimeSlotIsOverlapping_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var hallId = Guid.NewGuid();
        var hall = new Hall("Conference Room A", 50, 100m);
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var request = new CreateBookingRequest(hallId, startTime, 3m, null);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(hall);

        _bookingRepository.HasOverlappingBookingAsync(
            hall.Id, startTime, startTime.AddHours(3), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var act = () => _service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("The conference hall is already booked for the specified time slot.");

        await _bookingRepository.DidNotReceive().AddAsync(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WhenSelectedOptionIsNotSupportedByHall_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var hallId = Guid.NewGuid();
        var hall = new Hall("Conference Room A", 50, 100m);
        var unsupportedOptionId = Guid.NewGuid();

        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var request = new CreateBookingRequest(hallId, startTime, 2m, new List<Guid> { unsupportedOptionId });

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(hall);

        _bookingRepository.HasOverlappingBookingAsync(
            hall.Id, startTime, startTime.AddHours(2), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var act = () => _service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("One or more selected options are not available for this conference hall.");

        await _optionRepository.DidNotReceive().GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>());
        await _bookingRepository.DidNotReceive().AddAsync(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WhenOptionDoesNotExistInDb_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var hallId = Guid.NewGuid();
        var hall = new Hall("Conference Room A", 50, 100m);
        var optionId = Guid.NewGuid();
        hall.AddOption(optionId);

        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var request = new CreateBookingRequest(hallId, startTime, 2m, new List<Guid> { optionId });

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(hall);

        _bookingRepository.HasOverlappingBookingAsync(
            hall.Id, startTime, startTime.AddHours(2), Arg.Any<CancellationToken>())
            .Returns(false);

        _optionRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Option>());

        // Act
        var act = () => _service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("One or more specified options do not exist.");

        await _bookingRepository.DidNotReceive().AddAsync(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithoutOptions_ShouldCreateBookingAndReturnResponse()
    {
        // Arrange
        var hallId = Guid.NewGuid();
        var hall = new Hall("Conference Room A", 50, 100m);
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var durationHours = 2m;
        var endTime = startTime.AddHours((double)durationHours);

        var request = new CreateBookingRequest(hallId, startTime, durationHours, null);
        var pricingResult = new PricingResult(200m, 0m, 200m);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(hall);

        _bookingRepository.HasOverlappingBookingAsync(hall.Id, startTime, endTime, Arg.Any<CancellationToken>())
            .Returns(false);

        _pricingService.CalculatePrice(hall.BaseHourlyRate, Arg.Any<IReadOnlyList<Option>>(), startTime, endTime)
            .Returns(pricingResult);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.HallId.Should().Be(hall.Id);
        result.HallName.Should().Be("Conference Room A");
        result.HallCapacity.Should().Be(50);
        result.HallBaseHourlyRate.Should().Be(100m);
        result.StartTime.Should().Be(startTime);
        result.EndTime.Should().Be(endTime);
        result.DurationHours.Should().Be(2m);
        result.SelectedOptions.Should().BeEmpty();
        result.HallCost.Should().Be(200m);
        result.OptionsCost.Should().Be(0m);
        result.TotalCost.Should().Be(200m);

        await _bookingRepository.Received(1).AddAsync(
            Arg.Is((Booking b) => b.HallId == hall.Id && b.StartTime == startTime && b.EndTime == endTime),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithValidOptions_ShouldCalculatePriceAndCreateBooking()
    {
        // Arrange
        var hallId = Guid.NewGuid();
        var hall = new Hall("Conference Room A", 50, 100m);
        var option = new Option("Projector", 50m);
        hall.AddOption(option.Id);

        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var durationHours = 3m;
        var endTime = startTime.AddHours((double)durationHours);

        var request = new CreateBookingRequest(hallId, startTime, durationHours, new List<Guid> { option.Id });
        var pricingResult = new PricingResult(300m, 50m, 350m);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(hall);

        _bookingRepository.HasOverlappingBookingAsync(hall.Id, startTime, endTime, Arg.Any<CancellationToken>())
            .Returns(false);

        _optionRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Option> { option });

        _pricingService.CalculatePrice(hall.BaseHourlyRate, Arg.Is<IReadOnlyList<Option>>(l => l.Count == 1), startTime, endTime)
            .Returns(pricingResult);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.SelectedOptions.Should().HaveCount(1);
        result.SelectedOptions.First().Name.Should().Be("Projector");
        result.HallCost.Should().Be(300m);
        result.OptionsCost.Should().Be(50m);
        result.TotalCost.Should().Be(350m);

        await _bookingRepository.Received(1).AddAsync(
            Arg.Is((Booking b) => b.HallId == hall.Id && b.BookingOptions.Count == 1),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public async Task CreateAsync_WhenDurationIsZeroOrNegative_ShouldThrowBusinessRuleExceptionAndNotCallDb(decimal invalidDuration)
    {
        // Arrange
        var hallId = Guid.NewGuid();
        var request = new CreateBookingRequest(hallId, DateTimeOffset.UtcNow.AddDays(1), invalidDuration, null);

        // Act
        var act = () => _service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("Booking duration must be greater than zero.");

        await _hallRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _bookingRepository.DidNotReceive().HasOverlappingBookingAsync(
            Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion
}