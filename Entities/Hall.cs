namespace ConferenceHallBookingApi.Entities
{
    public class Hall
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public decimal BaseHourlyRate { get; set; }

        public ICollection<HallService> HallServices { get; set; } = new List<HallService>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
