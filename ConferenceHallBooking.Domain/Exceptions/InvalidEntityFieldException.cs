namespace ConferenceHallBooking.Domain.Exceptions;

public class InvalidEntityFieldException(string entityName, string fieldName, string reason)
    : BusinessRuleException(
        $"Invalid field '{fieldName}' for entity '{entityName}': {reason}.",
        "INVALID_ENTITY_FIELD")
{
    public string EntityName { get; } = entityName;
    public string FieldName { get; } = fieldName;
}
