namespace ConferenceHallBooking.Application.DTOs.Options;

public record OptionResponse(
    Guid Id,
    string Name,
    decimal Price);