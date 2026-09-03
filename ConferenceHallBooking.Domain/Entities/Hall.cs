namespace ConferenceHallBooking.Domain.Entities;

public class Hall
{
    public long Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int Capacity { get; private set; }
    public decimal BaseHourlyRate { get; private set; }

    private readonly List<HallOption> _hallOptions = new();
    public IReadOnlyCollection<HallOption> HallOptions => _hallOptions.AsReadOnly();

    private readonly List<Booking> _bookings = new();
    public IReadOnlyCollection<Booking> Bookings => _bookings.AsReadOnly();

    private Hall() { }

    public Hall(string name, int capacity, decimal baseHourlyRate)
    {
        Update(name, capacity, baseHourlyRate);
    }

    public void Update(string name, int capacity, decimal baseHourlyRate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hall name cannot be empty.", nameof(name));

        if (capacity <= 0)
            throw new ArgumentException("Capacity must be greater than zero.", nameof(capacity));

        if (baseHourlyRate < 0)
            throw new ArgumentException("Base hourly rate cannot be negative.", nameof(baseHourlyRate));

        Name = name;
        Capacity = capacity;
        BaseHourlyRate = baseHourlyRate;
    }

    public void AddOption(long optionId)
    {
        if (!_hallOptions.Any(o => o.OptionId == optionId))
        {
            _hallOptions.Add(new HallOption(Id, optionId));
        }
    }

    public void RemoveOption(long optionId)
    {
        var option = _hallOptions.FirstOrDefault(o => o.OptionId == optionId);
        if (option != null)
        {
            _hallOptions.Remove(option);
        }
    }
}