using ConferenceHallBookingApi.DTOs.Services;

namespace ConferenceHallBookingApi.DTOs.Halls;

public class HallResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal BaseHourlyRate { get; set; }
    public List<ServiceResponse> Services { get; set; } = new();
}