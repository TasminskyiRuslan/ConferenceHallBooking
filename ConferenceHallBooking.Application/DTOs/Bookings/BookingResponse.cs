using ConferenceHallBookingApi.ConferenceHallBooking.Application.DTOs.Options;

namespace ConferenceHallBookingApi.ConferenceHallBooking.Application.DTOs.Bookings;

public class BookingResponse
{
    public long Id { get; set; }
    public long HallId { get; set; }
    public string HallName { get; set; } = string.Empty;
    public int HallCapacity { get; set; }
    public decimal HallBaseHourlyRate { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public int DurationHours { get; set; }
    public List<OptionResponse> SelectedOptions { get; set; } = new();
    public decimal TotalPrice { get; set; }
}