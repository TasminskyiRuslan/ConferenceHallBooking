namespace ConferenceHallBooking.Domain.Exceptions;

public class NotFoundException(string message) : Exception(message);

public class NotFoundException<TEntity>(string key)
    : NotFoundException($"Entity \"{typeof(TEntity).Name}\" with key \"{key}\" was not found.")
{
    public string Key { get; } = key;
}
