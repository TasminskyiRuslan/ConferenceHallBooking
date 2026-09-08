using ConferenceHallBooking.Domain.Exceptions;

namespace ConferenceHallBooking.Domain.Entities;

/// <summary>
/// Join entity linking a booking to a selected service option.
/// Captures the option price at the time of booking to preserve historical accuracy.
/// </summary>
public class BookingOption
{
    /// <summary>Foreign key to the parent booking.</summary>
    public Guid BookingId { get; private set; }

    /// <summary>Navigation property to the parent booking.</summary>
    public Booking Booking { get; private set; } = null!;

    /// <summary>Foreign key to the selected service option.</summary>
    public Guid OptionId { get; private set; }

    /// <summary>Navigation property to the selected service option.</summary>
    public Option Option { get; private set; } = null!;

    /// <summary>Price of the option at the time the booking was created.</summary>
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
