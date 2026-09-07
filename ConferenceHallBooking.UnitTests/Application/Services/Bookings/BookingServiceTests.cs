using ConferenceHallBooking.Application.DTOs.Bookings;
using ConferenceHallBooking.Application.Interfaces.Bookings;
using ConferenceHallBooking.Application.Services.Bookings;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace ConferenceHallBooking.UnitTests.Application.Services.Bookings;

public class BookingServiceTests
{
    private readonly IBookingRepository _bookingRepository = Substitute.For<IBookingRepository>();
    private readonly IHallRepository _hallRepository = Substitute.For<IHallRepository>();
    private readonly IOptionRepository _optionRepository = Substitute.For<IOptionRepository>();
    private readonly IPricingService _pricingService = Substitute.For<IPricingService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly BookingService _service;

    public BookingServiceTests()
    {
        _service = new BookingService(
            _bookingRepository,
            _hallRepository,
            _optionRepository,
            _pricingService,
            _unitOfWork);
    }

    #region Validation

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    [InlineData(-0.5)]
    public async Task CreateAsync_WhenDurationIsInvalid_ShouldThrowWithoutCallingDb(decimal invalidDuration)
    {
        var hallId = Guid.NewGuid();
        var request = new CreateBookingRequest(hallId, DateTimeOffset.UtcNow.AddDays(1), invalidDuration, null);

        var act = () => _service.CreateAsync(request);

        await act.Should().ThrowAsync<InvalidBookingDurationException>();

        await _hallRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _bookingRepository.DidNotReceive().HasOverlappingBookingAsync(
            Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WhenHallDoesNotExist_ShouldThrowNotFoundException()
    {
        var hallId = Guid.NewGuid();
        var request = new CreateBookingRequest(hallId, DateTimeOffset.UtcNow.AddDays(1), 2m, null);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns((Hall?)null);

        var act = () => _service.CreateAsync(request);

        await act.Should().ThrowAsync<NotFoundException<Hall>>();

        await _bookingRepository.DidNotReceive().HasOverlappingBookingAsync(
            Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
        await _bookingRepository.DidNotReceive().AddAsync(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WhenTimeSlotIsOverlapping_ShouldThrowWithoutSaving()
    {
        var hallId = Guid.NewGuid();
        var hall = new Hall("Conference Room A", 50, 100m);
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var request = new CreateBookingRequest(hallId, startTime, 3m, null);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(hall);

        _bookingRepository.HasOverlappingBookingAsync(
            hall.Id, startTime, startTime.AddHours(3), Arg.Any<CancellationToken>())
            .Returns(true);

        var act = () => _service.CreateAsync(request);

        await act.Should().ThrowAsync<HallAlreadyBookedException>();

        await _bookingRepository.DidNotReceive().AddAsync(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WhenSelectedOptionIsNotSupportedByHall_ShouldThrowWithoutCallingOptionRepository()
    {
        var hallId = Guid.NewGuid();
        var hall = new Hall("Conference Room A", 50, 100m);
        var unsupportedOptionId = Guid.NewGuid();
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var request = new CreateBookingRequest(hallId, startTime, 2m, [unsupportedOptionId]);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(hall);

        _bookingRepository.HasOverlappingBookingAsync(
            hall.Id, startTime, startTime.AddHours(2), Arg.Any<CancellationToken>())
            .Returns(false);

        var act = () => _service.CreateAsync(request);

        await act.Should().ThrowAsync<HallOptionNotSupportedException>();

        await _optionRepository.DidNotReceive().GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
        await _bookingRepository.DidNotReceive().AddAsync(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WhenOptionDoesNotExistInDb_ShouldThrowWithoutSaving()
    {
        var hallId = Guid.NewGuid();
        var hall = new Hall("Conference Room A", 50, 100m);
        var optionId = Guid.NewGuid();
        hall.AddOption(optionId);

        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var request = new CreateBookingRequest(hallId, startTime, 2m, [optionId]);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(hall);

        _bookingRepository.HasOverlappingBookingAsync(
            hall.Id, startTime, startTime.AddHours(2), Arg.Any<CancellationToken>())
            .Returns(false);

        _optionRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var act = () => _service.CreateAsync(request);

        await act.Should().ThrowAsync<OptionsNotFoundException>();

        await _bookingRepository.DidNotReceive().AddAsync(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Happy Path

    [Fact]
    public async Task CreateAsync_WithoutOptions_ShouldCreateBookingAndReturnCompleteResponse()
    {
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

        var result = await _service.CreateAsync(request);

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
            Arg.Is<Booking>(b =>
                b.HallId == hall.Id &&
                b.StartTime == startTime &&
                b.EndTime == endTime &&
                b.TotalPrice == 200m &&
                b.BookingOptions.Count == 0),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithValidOptions_ShouldCalculatePriceAndCreateBooking()
    {
        var hallId = Guid.NewGuid();
        var hall = new Hall("Conference Room A", 50, 100m);
        var option = new Option("Projector", 50m);
        hall.AddOption(option.Id);

        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var durationHours = 3m;
        var endTime = startTime.AddHours((double)durationHours);

        var request = new CreateBookingRequest(hallId, startTime, durationHours, [option.Id]);
        var pricingResult = new PricingResult(300m, 50m, 350m);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(hall);

        _bookingRepository.HasOverlappingBookingAsync(hall.Id, startTime, endTime, Arg.Any<CancellationToken>())
            .Returns(false);

        _optionRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([option]);

        _pricingService.CalculatePrice(hall.BaseHourlyRate, Arg.Is<IReadOnlyList<Option>>(l => l.Count == 1), startTime, endTime)
            .Returns(pricingResult);

        var result = await _service.CreateAsync(request);

        result.Should().NotBeNull();
        result.SelectedOptions.Should().HaveCount(1);
        result.SelectedOptions.First().Name.Should().Be("Projector");
        result.HallCost.Should().Be(300m);
        result.OptionsCost.Should().Be(50m);
        result.TotalCost.Should().Be(350m);

        await _bookingRepository.Received(1).AddAsync(
            Arg.Is<Booking>(b =>
                b.HallId == hall.Id &&
                b.BookingOptions.Count == 1 &&
                b.BookingOptions.First().OptionId == option.Id &&
                b.BookingOptions.First().PriceAtBooking == 50m),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ShouldCallPricingServiceWithCorrectArguments()
    {
        var hallId = Guid.NewGuid();
        var hall = new Hall("Conference Room A", 50, 150m);
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var durationHours = 4m;
        var endTime = startTime.AddHours((double)durationHours);
        var request = new CreateBookingRequest(hallId, startTime, durationHours, null);
        var pricingResult = new PricingResult(600m, 0m, 600m);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(hall);

        _bookingRepository.HasOverlappingBookingAsync(hall.Id, startTime, endTime, Arg.Any<CancellationToken>())
            .Returns(false);

        _pricingService.CalculatePrice(150m, Arg.Any<IReadOnlyList<Option>>(), startTime, endTime)
            .Returns(pricingResult);

        await _service.CreateAsync(request);

        _pricingService.Received(1).CalculatePrice(150m, Arg.Any<IReadOnlyList<Option>>(), startTime, endTime);
    }

    [Fact]
    public async Task CreateAsync_WithMultipleOptions_ShouldPassAllOptionsToPricingService()
    {
        var hallId = Guid.NewGuid();
        var hall = new Hall("Conference Room A", 50, 100m);
        var option1 = new Option("Projector", 50m);
        var option2 = new Option("Wi-Fi", 30m);
        var option3 = new Option("Sound", 70m);

        hall.AddOption(option1.Id);
        hall.AddOption(option2.Id);
        hall.AddOption(option3.Id);

        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var request = new CreateBookingRequest(hallId, startTime, 2m, [option1.Id, option2.Id, option3.Id]);
        var pricingResult = new PricingResult(200m, 150m, 350m);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(hall);

        _bookingRepository.HasOverlappingBookingAsync(hall.Id, startTime, startTime.AddHours(2), Arg.Any<CancellationToken>())
            .Returns(false);

        _optionRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([option1, option2, option3]);

        _pricingService.CalculatePrice(hall.BaseHourlyRate, Arg.Is<IReadOnlyList<Option>>(l => l.Count == 3), startTime, startTime.AddHours(2))
            .Returns(pricingResult);

        var result = await _service.CreateAsync(request);

        result.SelectedOptions.Should().HaveCount(3);
        result.OptionsCost.Should().Be(150m);
        result.TotalCost.Should().Be(350m);
    }

    #endregion
}
