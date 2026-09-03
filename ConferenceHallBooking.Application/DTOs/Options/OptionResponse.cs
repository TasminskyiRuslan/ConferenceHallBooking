namespace ConferenceHallBookingApi.ConferenceHallBooking.Application.DTOs.Options;

public class OptionResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}