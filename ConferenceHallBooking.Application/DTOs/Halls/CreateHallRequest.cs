namespace ConferenceHallBookingApi.ConferenceHallBooking.Application.DTOs.Halls;

public class CreateHallRequest
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal BaseHourlyRate { get; set; }
    public List<long> OptionIds { get; set; } = new();
}