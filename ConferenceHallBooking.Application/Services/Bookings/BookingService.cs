using ConferenceHallBooking.Application.DTOs.Bookings;
using ConferenceHallBooking.Application.DTOs.Options;
using ConferenceHallBooking.Application.Exceptions;
using ConferenceHallBooking.Application.Interfaces.Bookings;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Interfaces;
using ConferenceHallBookingApi.ConferenceHallBooking.Application.Interfaces.Bookings;
using Microsoft.Extensions.Logging;

namespace ConferenceHallBooking.Application.Services.Bookings;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IHallRepository _hallRepository;
    private readonly IOptionRepository _optionRepository;
    private readonly IPricingService _pricingService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BookingService> _logger;

    public BookingService(
        IBookingRepository bookingRepository,
        IHallRepository hallRepository,
        IOptionRepository optionRepository,
        IPricingService pricingService,
        IUnitOfWork unitOfWork,
        ILogger<BookingService> logger)
    {
        _bookingRepository = bookingRepository;
        _hallRepository = hallRepository;
        _optionRepository = optionRepository;
        _pricingService = pricingService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BookingResponse> CreateAsync(
        CreateBookingRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.DurationHours <= 0)
        {
            _logger.LogWarning("Attempted to create a booking with invalid duration: {DurationHours} hours.", request.DurationHours);
            throw new BusinessRuleException("Booking duration must be greater than zero.");
        }

        var hall = await GetHallByIdOrThrowAsync(request.HallId, cancellationToken);

        var startTime = request.StartTime;
        var endTime = startTime.AddHours((double)request.DurationHours);

        var isOverlapping = await _bookingRepository.HasOverlappingBookingAsync(
            hall.Id, startTime, endTime, cancellationToken);

        if (isOverlapping)
        {
            _logger.LogWarning("Conference hall {HallId} is already booked for time slot between {StartTime} and {EndTime}.", hall.Id, startTime, endTime);
            throw new BusinessRuleException("The conference hall is already booked for the specified time slot.");
        }

        var targetOptionIds = (request.OptionIds ?? []).Distinct().ToList();
        var selectedOptions = new List<Option>();

        if (targetOptionIds.Count > 0)
        {
            ValidateHallSupportsOptions(hall, targetOptionIds);
            selectedOptions = (await GetOptionsOrThrowAsync(targetOptionIds, cancellationToken)).ToList();
        }

        var pricing = _pricingService.CalculatePrice(hall.BaseHourlyRate, selectedOptions, startTime, endTime);

        var bookingOptions = selectedOptions.Select(o => new BookingOption(o.Id, o.Price));
        var booking = new Booking(hall.Id, startTime, endTime, pricing.TotalCost, bookingOptions);

        await _bookingRepository.AddAsync(booking, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Booking {BookingId} was successfully created for hall {HallId}.", booking.Id, hall.Id);

        return MapToResponse(booking, hall, selectedOptions, pricing, request.DurationHours);
    }

    private async Task<Hall> GetHallByIdOrThrowAsync(Guid hallId, CancellationToken cancellationToken)
    {
        var hall = await _hallRepository.GetByIdAsync(hallId, cancellationToken);

        if (hall is null)
        {
            _logger.LogWarning("Conference hall with ID {HallId} was not found.", hallId);
            throw new NotFoundException($"Conference hall with ID '{hallId}' was not found.");
        }

        return hall;
    }

    private async Task<IReadOnlyList<Option>> GetOptionsOrThrowAsync(
        IReadOnlyCollection<Guid> distinctOptionIds,
        CancellationToken cancellationToken)
    {
        var existingOptions = (await _optionRepository.GetByIdsAsync(distinctOptionIds, cancellationToken)).ToList();

        if (existingOptions.Count != distinctOptionIds.Count)
        {
            _logger.LogWarning("Attempted to access one or more non-existent options.");
            throw new BusinessRuleException("One or more specified options do not exist.");
        }

        return existingOptions;
    }

    private static void ValidateHallSupportsOptions(Hall hall, IReadOnlyCollection<Guid> targetOptionIds)
    {
        var allowedOptionIds = hall.HallOptions.Select(ho => ho.OptionId).ToHashSet();
        var hasUnsupportedOptions = targetOptionIds.Any(id => !allowedOptionIds.Contains(id));

        if (hasUnsupportedOptions)
        {
            throw new BusinessRuleException("One or more selected options are not available for this conference hall.");
        }
    }

    private static BookingResponse MapToResponse(
        Booking booking,
        Hall hall,
        IReadOnlyCollection<Option> options,
        PricingResult pricing,
        decimal durationHours)
    {
        var optionResponses = options
            .Select(o => new OptionResponse(o.Id, o.Name, o.Price))
            .ToList();

        return new BookingResponse(
            booking.Id,
            hall.Id,
            hall.Name,
            hall.Capacity,
            hall.BaseHourlyRate,
            booking.StartTime,
            booking.EndTime,
            durationHours,
            optionResponses,
            pricing.HallCost,
            pricing.OptionsCost,
            pricing.TotalCost);
    }
}