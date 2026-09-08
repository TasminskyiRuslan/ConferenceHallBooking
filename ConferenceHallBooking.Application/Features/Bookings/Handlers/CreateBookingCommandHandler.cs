using ConferenceHallBooking.Application.DTOs.Bookings;
using ConferenceHallBooking.Application.Extensions;
using ConferenceHallBooking.Application.Features.Bookings.Commands;
using ConferenceHallBooking.Application.Interfaces.Bookings;
using ConferenceHallBooking.Application.Mappers;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;
using MediatR;

namespace ConferenceHallBooking.Application.Features.Bookings.Handlers;

public class CreateBookingCommandHandler(
    IBookingRepository bookingRepository,
    IHallRepository hallRepository,
    IOptionRepository optionRepository,
    IPricingService pricingService,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateBookingCommand, BookingResponse>
{
    public async Task<BookingResponse> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
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

        return BookingMapper.MapToResponse(booking, hall, selectedOptions, pricing, request.DurationHours);
    }
}
