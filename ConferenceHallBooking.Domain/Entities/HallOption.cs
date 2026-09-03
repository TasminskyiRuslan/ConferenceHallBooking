namespace ConferenceHallBooking.Domain.Entities;

public class HallOption
{
    public long HallId { get; private set; }
    public Hall Hall { get; private set; } = null!;

    public long OptionId { get; private set; }
    public Option Option { get; private set; } = null!;

    private HallOption() { }

    public HallOption(long hallId, long optionId)
    {
        HallId = hallId;
        OptionId = optionId;
    }
}