using ConferenceHallBooking.Domain.Exceptions;

namespace ConferenceHallBooking.Domain.Entities;

public class Option
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }

    private readonly List<HallOption> _hallOptions = [];
    public IReadOnlyCollection<HallOption> HallOptions => _hallOptions;

    private Option() { }

    public Option(string name, decimal price)
    {
        Id = Guid.NewGuid();
        Update(name, price);
    }

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
