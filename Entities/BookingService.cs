namespace ConferenceHallBookingApi.Entities
{
    public class BookingService
    {
        public long BookingId { get; set; }
        public Booking Booking { get; set; } = null!;

        public long ServiceId { get; set; }
        public Service Service { get; set; } = null!;
    }
}
