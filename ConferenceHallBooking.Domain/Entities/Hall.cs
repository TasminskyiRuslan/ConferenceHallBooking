using ConferenceHallBooking.Domain.Exceptions;

namespace ConferenceHallBooking.Domain.Entities;

public class Hall
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int Capacity { get; private set; }
    public decimal BaseHourlyRate { get; private set; }

    private readonly List<HallOption> _hallOptions = [];
    public IReadOnlyCollection<HallOption> HallOptions => _hallOptions;

    private readonly List<Booking> _bookings = [];
    public IReadOnlyCollection<Booking> Bookings => _bookings;

    private Hall() { }

    public Hall(string name, int capacity, decimal baseHourlyRate)
    {
        Id = Guid.NewGuid();
        Update(name, capacity, baseHourlyRate);
    }

    public void Update(string name, int capacity, decimal baseHourlyRate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidEntityFieldException(nameof(Hall), nameof(Name), "name cannot be empty");

        if (capacity <= 0)
            throw new InvalidEntityFieldException(nameof(Hall), nameof(Capacity), "capacity must be greater than zero");

        if (baseHourlyRate < 0)
            throw new InvalidEntityFieldException(nameof(Hall), nameof(BaseHourlyRate), "base hourly rate cannot be negative");

        Name = name;
        Capacity = capacity;
        BaseHourlyRate = baseHourlyRate;
    }

    public void AddOption(Guid optionId)
    {
        if (!_hallOptions.Any(o => o.OptionId == optionId))
        {
            _hallOptions.Add(new HallOption(Id, optionId));
        }
    }

    public void RemoveOption(Guid optionId)
    {
        var option = _hallOptions.FirstOrDefault(o => o.OptionId == optionId);
        if (option is not null)
        {
            _hallOptions.Remove(option);
        }
    }
}
