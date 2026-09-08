namespace ConferenceHallBooking.Application.DTOs.Reports;

/// <summary>
/// Revenue breakdown by hall for a given time period.
/// </summary>
public record RevenueReport(
    DateTimeOffset From,
    DateTimeOffset To,
    decimal TotalRevenue,
    IReadOnlyCollection<HallRevenue> ByHall);

public record HallRevenue(
    Guid HallId,
    string HallName,
    int BookingCount,
    decimal Revenue);
