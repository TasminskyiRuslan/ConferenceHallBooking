namespace ConferenceHallBookingApi.Entities;

public class HallOption
{
    public long HallId { get; set; }
    public Hall Hall { get; set; } = null!;

    public long OptionId { get; set; }
    public Option Option { get; set; } = null!;
}