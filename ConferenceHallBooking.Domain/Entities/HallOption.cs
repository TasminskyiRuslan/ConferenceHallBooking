namespace ConferenceHallBooking.Domain.Entities;

/// <summary>
/// Join entity linking a hall to an available service option (e.g., projector, Wi-Fi).
/// </summary>
public class HallOption
{
    /// <summary>Foreign key to the hall.</summary>
    public Guid HallId { get; private set; }

    /// <summary>Navigation property to the hall.</summary>
    public Hall Hall { get; private set; } = null!;

    /// <summary>Foreign key to the service option.</summary>
    public Guid OptionId { get; private set; }

    /// <summary>Navigation property to the service option.</summary>
    public Option Option { get; private set; } = null!;

    private HallOption() { }

    public HallOption(Guid hallId, Guid optionId)
    {
        HallId = hallId;
        OptionId = optionId;
    }
}
