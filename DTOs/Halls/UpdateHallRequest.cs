namespace ConferenceHallBookingApi.DTOs.Halls;

public class UpdateHallRequest
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal BaseHourlyRate { get; set; }
    public List<long> ServiceIds { get; set; } = new();
}