namespace ConferenceHallBooking.Domain.Exceptions;

public abstract class BusinessRuleException(string message, string errorCode) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}
