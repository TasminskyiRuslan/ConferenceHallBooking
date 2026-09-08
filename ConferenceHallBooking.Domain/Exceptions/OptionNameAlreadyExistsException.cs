namespace ConferenceHallBooking.Domain.Exceptions;

/// <summary>
/// Thrown when attempting to create or update an option with a name that already exists.
/// </summary>
public class OptionNameAlreadyExistsException(string name)
    : BusinessRuleException(
        $"An option with the name '{name}' already exists.",
        "OPTION_NAME_ALREADY_EXISTS");
