using ConferenceHallBooking.Domain.Exceptions;

namespace ConferenceHallBooking.Domain.Entities;

public class BookingOption
{
    public Guid BookingId { get; private set; }
    public Booking Booking { get; private set; } = null!;

    public Guid OptionId { get; private set; }
    public Option Option { get; private set; } = null!;

    public decimal PriceAtBooking { get; private set; }

    private BookingOption() { }

    public BookingOption(Guid optionId, decimal priceAtBooking)
    {
        if (priceAtBooking < 0)
            throw new InvalidEntityFieldException(nameof(BookingOption), nameof(PriceAtBooking), "price cannot be negative");

        OptionId = optionId;
        PriceAtBooking = priceAtBooking;
    }
}
