namespace ConferenceHallBooking.Domain.Entities;

public class BookingOption
{
    public long BookingId { get; private set; }
    public Booking Booking { get; private set; } = null!;

    public long OptionId { get; private set; }
    public Option Option { get; private set; } = null!;

    // Фіксуємо ціну опції НА МОМЕНТ БРОНЮВАННЯ!
    public decimal PriceAtBooking { get; private set; }

    private BookingOption() { }

    public BookingOption(long optionId, decimal priceAtBooking)
    {
        if (priceAtBooking < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(priceAtBooking));

        OptionId = optionId;
        PriceAtBooking = priceAtBooking;
    }
}