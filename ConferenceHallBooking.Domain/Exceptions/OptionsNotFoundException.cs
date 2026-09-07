namespace ConferenceHallBooking.Domain.Exceptions;

public class OptionsNotFoundException(IEnumerable<Guid> optionIds)
    : BusinessRuleException(
        $"One or more specified options do not exist. Requested IDs: [{string.Join(", ", optionIds)}].",
        "OPTIONS_NOT_FOUND")
{
    public IReadOnlyCollection<Guid> OptionIds { get; } = optionIds.ToList().AsReadOnly();
}
