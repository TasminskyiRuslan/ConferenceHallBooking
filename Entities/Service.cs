namespace ConferenceHallBookingApi.Entities
{
    public class Service
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public ICollection<HallService> HallServices { get; set; } = new List<HallService>();
    }
}
