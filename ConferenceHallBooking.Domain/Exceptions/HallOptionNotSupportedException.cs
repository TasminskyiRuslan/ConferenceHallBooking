namespace ConferenceHallBooking.Domain.Exceptions;

public class HallOptionNotSupportedException(Guid hallId, IEnumerable<Guid> optionIds)
    : BusinessRuleException(
        $"One or more selected options are not available for conference hall '{hallId}'. Unsupported option IDs: [{string.Join(", ", optionIds)}].",
        "HALL_OPTION_NOT_SUPPORTED")
{
    public Guid HallId { get; } = hallId;
    public IReadOnlyCollection<Guid> OptionIds { get; } = optionIds.ToList().AsReadOnly();
}
