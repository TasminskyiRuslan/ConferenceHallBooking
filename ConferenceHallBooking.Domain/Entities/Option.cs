using ConferenceHallBooking.Domain.Exceptions;

namespace ConferenceHallBooking.Domain.Entities;

/// <summary>
/// Represents a bookable service option (e.g., projector, Wi-Fi, sound system).
/// </summary>
public class Option
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; private set; }

    /// <summary>Display name of the option (e.g., "Проєктор").</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Fixed price per booking in UAH. Zero is allowed (free option).</summary>
    public decimal Price { get; private set; }

    private readonly List<HallOption> _hallOptions = [];
    /// <summary>Halls that offer this option.</summary>
    public IReadOnlyCollection<HallOption> HallOptions => _hallOptions;

    private Option() { }

    public Option(string name, decimal price)
    {
        Id = Guid.NewGuid();
        Update(name, price);
    }

    /// <summary>
    /// Updates option properties. Throws <see cref="InvalidEntityFieldException"/> on invalid input.
    /// </summary>
    public void Update(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidEntityFieldException(nameof(Option), nameof(Name), "name cannot be empty");

        if (price < 0)
            throw new InvalidEntityFieldException(nameof(Option), nameof(Price), "price cannot be negative");

        Name = name;
        Price = price;
    }
}
