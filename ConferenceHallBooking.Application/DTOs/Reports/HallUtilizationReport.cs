namespace ConferenceHallBooking.Application.DTOs.Reports;

/// <summary>
/// Hall utilization statistics for a given time period.
/// </summary>
public record HallUtilizationReport(
    DateTimeOffset From,
    DateTimeOffset To,
    IReadOnlyCollection<HallUtilization> Halls);

public record HallUtilization(
    Guid HallId,
    string HallName,
    int Capacity,
    decimal BookedHours,
    decimal AvailableHours,
    decimal UtilizationPercent);
