namespace ConferenceHallBookingApi.DTOs.Bookings;

public class CreateBookingRequest
{
    public long HallId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public int DurationHours { get; set; }
    public List<long> ServiceIds { get; set; } = new();
}