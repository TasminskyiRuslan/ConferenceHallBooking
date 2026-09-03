using ConferenceHallBooking.Domain.Entities;

namespace ConferenceHallBooking.Domain.Entities;

public class Option
{
    public long Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }

    private readonly List<HallOption> _hallOptions = new();
    public IReadOnlyCollection<HallOption> HallOptions => _hallOptions.AsReadOnly();

    private Option() { }

    public Option(string name, decimal price)
    {
        UpdateDetails(name, price);
    }

    public void UpdateDetails(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Option name cannot be empty.", nameof(name));

        if (price < 0)
            throw new ArgumentException("Option price cannot be negative.", nameof(price));

        Name = name;
        Price = price;
    }
}