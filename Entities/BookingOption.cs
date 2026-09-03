namespace ConferenceHallBookingApi.Entities;

public class BookingOption
{
    public long BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    public long OptionId { get; set; }
    public Option Option { get; set; } = null!;
}