namespace ConferenceHallBookingApi.Entities
{
    public class Booking
    {
        public long Id { get; set; }

        public long HallId { get; set; }
        public Hall Hall { get; set; } = null!;

        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }

        public decimal TotalPrice { get; set; }

        public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
    }
}
