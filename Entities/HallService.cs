namespace ConferenceHallBookingApi.Entities
{
    public class HallService
    {
        public long HallId { get; set; }
        public Hall Hall { get; set; } = null!;

        public long ServiceId { get; set; }
        public Service Service { get; set; } = null!;
    }
}
