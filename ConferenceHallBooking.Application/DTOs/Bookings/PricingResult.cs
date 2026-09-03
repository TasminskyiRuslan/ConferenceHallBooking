namespace ConferenceHallBooking.Application.DTOs.Bookings;

public record PricingResult(
    decimal HallCost,
    decimal OptionsCost,
    decimal TotalCost);