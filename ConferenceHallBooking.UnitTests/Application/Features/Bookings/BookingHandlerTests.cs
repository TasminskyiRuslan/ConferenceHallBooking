using ConferenceHallBooking.Application.DTOs.Bookings;
using ConferenceHallBooking.Application.Features.Bookings.Commands;
using ConferenceHallBooking.Application.Features.Bookings.Handlers;
using ConferenceHallBooking.Application.Features.Bookings.Queries;
using ConferenceHallBooking.Application.Interfaces.Bookings;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace ConferenceHallBooking.UnitTests.Application.Features.Bookings;

public class BookingHandlerTests
{
    private readonly IBookingRepository _bookingRepository = Substitute.For<IBookingRepository>();
    private readonly IHallRepository _hallRepository = Substitute.For<IHallRepository>();
    private readonly IOptionRepository _optionRepository = Substitute.For<IOptionRepository>();
    private readonly IPricingService _pricingService = Substitute.For<IPricingService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    #region CreateBookingCommandHandler

    [Fact]
    public async Task CreateBookingCommandHandler_WhenHallDoesNotExist_ShouldThrowNotFoundException()
    {
        var handler = new CreateBookingCommandHandler(
            _bookingRepository, _hallRepository, _optionRepository, _pricingService, _unitOfWork);

        var hallId = Guid.NewGuid();
        var command = new CreateBookingCommand(hallId, DateTimeOffset.UtcNow.AddDays(1), 2m, null);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns((Hall?)null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException<Hall>>();

        await _bookingRepository.DidNotReceive().HasOverlappingBookingAsync(
            Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateBookingCommandHandler_WhenTimeSlotIsOverlapping_ShouldThrowWithoutSaving()
    {
        var handler = new CreateBookingCommandHandler(
            _bookingRepository, _hallRepository, _optionRepository, _pricingService, _unitOfWork);

        var hallId = Guid.NewGuid();
        var hall = new Hall("Conference Room A", 50, 100m);
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var command = new CreateBookingCommand(hallId, startTime, 3m, null);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(hall);

        _bookingRepository.HasOverlappingBookingAsync(
            hall.Id, startTime, startTime.AddHours(3), Arg.Any<CancellationToken>())
            .Returns(true);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<HallAlreadyBookedException>();

        await _bookingRepository.DidNotReceive().AddAsync(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateBookingCommandHandler_WhenSelectedOptionIsNotSupportedByHall_ShouldThrowWithoutCallingOptionRepository()
    {
        var handler = new CreateBookingCommandHandler(
            _bookingRepository, _hallRepository, _optionRepository, _pricingService, _unitOfWork);

        var hallId = Guid.NewGuid();
        var hall = new Hall("Conference Room A", 50, 100m);
        var unsupportedOptionId = Guid.NewGuid();
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var command = new CreateBookingCommand(hallId, startTime, 2m, [unsupportedOptionId]);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(hall);

        _bookingRepository.HasOverlappingBookingAsync(
            hall.Id, startTime, startTime.AddHours(2), Arg.Any<CancellationToken>())
            .Returns(false);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<HallOptionNotSupportedException>();

        await _optionRepository.DidNotReceive().GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
        await _bookingRepository.DidNotReceive().AddAsync(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateBookingCommandHandler_WhenOptionDoesNotExistInDb_ShouldThrowWithoutSaving()
    {
        var handler = new CreateBookingCommandHandler(
            _bookingRepository, _hallRepository, _optionRepository, _pricingService, _unitOfWork);

        var hallId = Guid.NewGuid();
        var hall = new Hall("Conference Room A", 50, 100m);
        var optionId = Guid.NewGuid();
        hall.AddOption(optionId);

        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var command = new CreateBookingCommand(hallId, startTime, 2m, [optionId]);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(hall);

        _bookingRepository.HasOverlappingBookingAsync(
            hall.Id, startTime, startTime.AddHours(2), Arg.Any<CancellationToken>())
            .Returns(false);

        _optionRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<OptionsNotFoundException>();

        await _bookingRepository.DidNotReceive().AddAsync(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateBookingCommandHandler_WithoutOptions_ShouldCreateBookingAndReturnCompleteResponse()
    {
        var handler = new CreateBookingCommandHandler(
            _bookingRepository, _hallRepository, _optionRepository, _pricingService, _unitOfWork);

        var hallId = Guid.NewGuid();
        var hall = new Hall("Conference Room A", 50, 100m);
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var durationHours = 2m;
        var endTime = startTime.AddHours((double)durationHours);

        var command = new CreateBookingCommand(hallId, startTime, durationHours, null);
        var pricingResult = new PricingResult(200m, 0m, 200m);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(hall);

        _bookingRepository.HasOverlappingBookingAsync(hall.Id, startTime, endTime, Arg.Any<CancellationToken>())
            .Returns(false);

        _pricingService.CalculatePrice(hall.BaseHourlyRate, Arg.Any<IReadOnlyList<Option>>(), startTime, endTime)
            .Returns(pricingResult);

        var result = await handler.Handle(command, CancellationToken.None);

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
    public async Task CreateBookingCommandHandler_WithValidOptions_ShouldCalculatePriceAndCreateBooking()
    {
        var handler = new CreateBookingCommandHandler(
            _bookingRepository, _hallRepository, _optionRepository, _pricingService, _unitOfWork);

        var hallId = Guid.NewGuid();
        var hall = new Hall("Conference Room A", 50, 100m);
        var option = new Option("Projector", 50m);
        hall.AddOption(option.Id);

        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var durationHours = 3m;
        var endTime = startTime.AddHours((double)durationHours);

        var command = new CreateBookingCommand(hallId, startTime, durationHours, [option.Id]);
        var pricingResult = new PricingResult(300m, 50m, 350m);

        _hallRepository.GetByIdAsync(hallId, Arg.Any<CancellationToken>())
            .Returns(hall);

        _bookingRepository.HasOverlappingBookingAsync(hall.Id, startTime, endTime, Arg.Any<CancellationToken>())
            .Returns(false);

        _optionRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([option]);

        _pricingService.CalculatePrice(hall.BaseHourlyRate, Arg.Is<IReadOnlyList<Option>>(l => l.Count == 1), startTime, endTime)
            .Returns(pricingResult);

        var result = await handler.Handle(command, CancellationToken.None);

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

    #endregion

    #region GetBookingByIdQueryHandler

    [Fact]
    public async Task GetBookingByIdQueryHandler_WhenBookingExists_ShouldReturnBookingResponse()
    {
        var handler = new GetBookingByIdQueryHandler(_bookingRepository, _hallRepository);

        var hall = new Hall("Conference Room A", 50, 100m);
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var endTime = startTime.AddHours(3);
        var option = new Option("Projector", 50m);
        var bookingOption = new BookingOption(option.Id, 50m);
        var booking = new Booking(hall.Id, startTime, endTime, 350m, [bookingOption]);

        _bookingRepository.GetByIdAsync(booking.Id, Arg.Any<CancellationToken>())
            .Returns(booking);

        _hallRepository.GetByIdAsync(hall.Id, Arg.Any<CancellationToken>())
            .Returns(hall);

        var result = await handler.Handle(new GetBookingByIdQuery(booking.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(booking.Id);
        result.HallId.Should().Be(hall.Id);
        result.HallName.Should().Be("Conference Room A");
        result.HallCapacity.Should().Be(50);
        result.HallBaseHourlyRate.Should().Be(100m);
        result.StartTime.Should().Be(startTime);
        result.EndTime.Should().Be(endTime);
        result.DurationHours.Should().Be(3m);
        result.HallCost.Should().Be(300m);
        result.OptionsCost.Should().Be(50m);
        result.TotalCost.Should().Be(350m);
    }

    [Fact]
    public async Task GetBookingByIdQueryHandler_WhenBookingDoesNotExist_ShouldThrowNotFoundException()
    {
        var handler = new GetBookingByIdQueryHandler(_bookingRepository, _hallRepository);
        var bookingId = Guid.NewGuid();

        _bookingRepository.GetByIdAsync(bookingId, Arg.Any<CancellationToken>())
            .Returns((Booking?)null);

        var act = () => handler.Handle(new GetBookingByIdQuery(bookingId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException<Booking>>();
    }

    [Fact]
    public async Task GetBookingByIdQueryHandler_WhenHallDoesNotExist_ShouldThrowNotFoundException()
    {
        var handler = new GetBookingByIdQueryHandler(_bookingRepository, _hallRepository);

        var hall = new Hall("Conference Room A", 50, 100m);
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var endTime = startTime.AddHours(3);
        var bookingOption = new BookingOption(Guid.NewGuid(), 50m);
        var booking = new Booking(hall.Id, startTime, endTime, 350m, [bookingOption]);

        _bookingRepository.GetByIdAsync(booking.Id, Arg.Any<CancellationToken>())
            .Returns(booking);

        _hallRepository.GetByIdAsync(hall.Id, Arg.Any<CancellationToken>())
            .Returns((Hall?)null);

        var act = () => handler.Handle(new GetBookingByIdQuery(booking.Id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException<Hall>>();
    }

    #endregion
}
