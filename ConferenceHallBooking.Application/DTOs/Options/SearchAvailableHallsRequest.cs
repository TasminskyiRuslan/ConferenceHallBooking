namespace ConferenceHallBookingApi.ConferenceHallBooking.Application.DTOs.Options;

public class SearchAvailableHallsRequest
{
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int Capacity { get; set; }
}