namespace ConferenceHallBooking.Application.DTOs.Options;

/// <summary>
/// Request model for updating an existing service option.
/// </summary>
public record UpdateOptionRequest(
    string Name,
    decimal Price);
