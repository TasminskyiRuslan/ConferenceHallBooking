namespace ConferenceHallBooking.Application.DTOs.Reports;

/// <summary>
/// Overall booking summary for a given time period.
/// </summary>
public record BookingSummaryReport(
    DateTimeOffset From,
    DateTimeOffset To,
    int TotalBookings,
    decimal TotalRevenue,
    decimal AverageBookingDurationHours,
    decimal AverageBookingRevenue,
    IReadOnlyCollection<PopularTimeSlot> PopularTimeSlots);

public record PopularTimeSlot(
    int Hour,
    int BookingCount);
