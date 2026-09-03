namespace ConferenceHallBookingApi.Entities;

public class Option
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public ICollection<HallOption> HallOptions { get; set; } = new List<HallOption>();
}