using ConferenceHallBooking.Application.DTOs.Bookings;
using ConferenceHallBooking.Application.DTOs.Options;
using ConferenceHallBooking.Application.Extensions;
using ConferenceHallBooking.Application.Interfaces.Bookings;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;

namespace ConferenceHallBooking.Application.Services.Bookings;

public class BookingService(
    IBookingRepository bookingRepository,
    IHallRepository hallRepository,
    IOptionRepository optionRepository,
    IPricingService pricingService,
    IUnitOfWork unitOfWork) : IBookingService
{
    public async Task<BookingResponse> CreateAsync(
        CreateBookingRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.DurationHours <= 0)
        {
            throw new InvalidBookingDurationException(request.DurationHours);
        }

        var hall = await hallRepository.GetByIdAsync(request.HallId, cancellationToken)
            ?? throw new NotFoundException<Hall>(request.HallId.ToString());

        var startTime = request.StartTime;
        var endTime = startTime.AddHours((double)request.DurationHours);

        if (await bookingRepository.HasOverlappingBookingAsync(hall.Id, startTime, endTime, cancellationToken))
        {
            throw new HallAlreadyBookedException(hall.Id, startTime, endTime);
        }

        var optionIds = new HashSet<Guid>(request.OptionIds ?? []);

        var allowedOptionIds = hall.HallOptions.Select(ho => ho.OptionId).ToHashSet();

        var unsupportedOptionIds = optionIds.Where(id => !allowedOptionIds.Contains(id)).ToList();

        if (unsupportedOptionIds.Count != 0)
        {
            throw new HallOptionNotSupportedException(hall.Id, unsupportedOptionIds);
        }

        var selectedOptions = await optionRepository.GetByIdsOrThrowAsync(optionIds, cancellationToken);

        var pricing = pricingService.CalculatePrice(hall.BaseHourlyRate, selectedOptions, startTime, endTime);

        var bookingOptions = selectedOptions.Select(o => new BookingOption(o.Id, o.Price));
        var booking = new Booking(hall.Id, startTime, endTime, pricing.TotalCost, bookingOptions);

        await bookingRepository.AddAsync(booking, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToResponse(booking, hall, selectedOptions, pricing, request.DurationHours);
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
