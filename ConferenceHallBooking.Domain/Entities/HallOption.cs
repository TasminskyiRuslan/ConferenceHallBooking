namespace ConferenceHallBooking.Domain.Entities;

public class HallOption
{
    public Guid HallId { get; private set; }
    public Hall Hall { get; private set; } = null!;

    public Guid OptionId { get; private set; }
    public Option Option { get; private set; } = null!;

    private HallOption() { }

    public HallOption(Guid hallId, Guid optionId)
    {
        HallId = hallId;
        OptionId = optionId;
    }
}