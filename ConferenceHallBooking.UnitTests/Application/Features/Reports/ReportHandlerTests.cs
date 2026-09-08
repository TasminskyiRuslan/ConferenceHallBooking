using ConferenceHallBooking.Application.DTOs.Reports;
using ConferenceHallBooking.Application.Features.Reports.Handlers;
using ConferenceHallBooking.Application.Features.Reports.Queries;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Infrastructure.Data;
using ConferenceHallBooking.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHallBooking.UnitTests.Application.Features.Reports;

public class ReportHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly GetRevenueReportHandler _revenueHandler;
    private readonly GetHallUtilizationReportHandler _utilizationHandler;
    private readonly GetBookingSummaryReportHandler _summaryHandler;

    public ReportHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        var bookingRepo = new BookingRepository(_context);
        var hallRepo = new HallRepository(_context);

        _revenueHandler = new GetRevenueReportHandler(bookingRepo);
        _utilizationHandler = new GetHallUtilizationReportHandler(hallRepo);
        _summaryHandler = new GetBookingSummaryReportHandler(bookingRepo);
    }

    public void Dispose() => _context.Dispose();

    #region Revenue Report

    [Fact]
    public async Task GetRevenueReportHandler_WithBookings_ShouldReturnCorrectRevenue()
    {
        var hall = SeedHall("Hall A", 50, 1000m);
        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);

        SeedBooking(hall.Id, from.AddDays(1), from.AddDays(1).AddHours(2), 2000m);
        SeedBooking(hall.Id, from.AddDays(5), from.AddDays(5).AddHours(3), 3000m);

        var report = await _revenueHandler.Handle(
            new GetRevenueReportQuery(from, to), CancellationToken.None);

        report.TotalRevenue.Should().Be(5000m);
        report.ByHall.Should().HaveCount(1);
        report.ByHall.First().BookingCount.Should().Be(2);
    }

    [Fact]
    public async Task GetRevenueReportHandler_WithNoBookings_ShouldReturnZeroRevenue()
    {
        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);

        var report = await _revenueHandler.Handle(
            new GetRevenueReportQuery(from, to), CancellationToken.None);

        report.TotalRevenue.Should().Be(0);
        report.ByHall.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRevenueReportHandler_ShouldOnlyIncludeBookingsInRange()
    {
        var hall = SeedHall("Hall A", 50, 1000m);
        var from = new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 1, 20, 0, 0, 0, TimeSpan.Zero);

        SeedBooking(hall.Id, from.AddDays(-5), from.AddDays(-5).AddHours(2), 1000m);
        SeedBooking(hall.Id, from.AddDays(2), from.AddDays(2).AddHours(2), 2000m);
        SeedBooking(hall.Id, to.AddDays(5), to.AddDays(5).AddHours(2), 3000m);

        var report = await _revenueHandler.Handle(
            new GetRevenueReportQuery(from, to), CancellationToken.None);

        report.TotalRevenue.Should().Be(2000m);
        report.ByHall.First().BookingCount.Should().Be(1);
    }

    #endregion

    #region Utilization Report

    [Fact]
    public async Task GetUtilizationReportHandler_WithBookings_ShouldCalculateUtilization()
    {
        var hall = SeedHall("Hall A", 50, 1000m);
        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 1, 8, 0, 0, 0, TimeSpan.Zero);

        SeedBooking(hall.Id, from.AddDays(1), from.AddDays(2), 1000m);

        var report = await _utilizationHandler.Handle(
            new GetHallUtilizationReportQuery(from, to), CancellationToken.None);

        report.Halls.Should().HaveCount(1);
        report.Halls.First().BookedHours.Should().Be(24);
        report.Halls.First().UtilizationPercent.Should().Be(14.3m);
    }

    [Fact]
    public async Task GetUtilizationReportHandler_WithNoHalls_ShouldReturnEmpty()
    {
        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 1, 8, 0, 0, 0, TimeSpan.Zero);

        var report = await _utilizationHandler.Handle(
            new GetHallUtilizationReportQuery(from, to), CancellationToken.None);

        report.Halls.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUtilizationReportHandler_WithMultipleHalls_ShouldOrderDescending()
    {
        var hallA = SeedHall("Hall A", 50, 1000m);
        var hallB = SeedHall("Hall B", 50, 1000m);
        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 1, 8, 0, 0, 0, TimeSpan.Zero);

        SeedBooking(hallA.Id, from.AddDays(1), from.AddDays(2), 1000m);
        SeedBooking(hallB.Id, from.AddDays(1), from.AddDays(4), 1000m);

        var report = await _utilizationHandler.Handle(
            new GetHallUtilizationReportQuery(from, to), CancellationToken.None);

        report.Halls.First().HallId.Should().Be(hallB.Id);
        report.Halls.Last().HallId.Should().Be(hallA.Id);
    }

    #endregion

    #region Booking Summary Report

    [Fact]
    public async Task GetBookingSummaryReportHandler_WithBookings_ShouldReturnCorrectSummary()
    {
        var hall = SeedHall("Hall A", 50, 1000m);
        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);

        SeedBooking(hall.Id, from.AddDays(1).AddHours(10), from.AddDays(1).AddHours(13), 3000m);
        SeedBooking(hall.Id, from.AddDays(5).AddHours(10), from.AddDays(5).AddHours(12), 2000m);

        var report = await _summaryHandler.Handle(
            new GetBookingSummaryReportQuery(from, to), CancellationToken.None);

        report.TotalBookings.Should().Be(2);
        report.TotalRevenue.Should().Be(5000m);
        report.AverageBookingDurationHours.Should().Be(2.5m);
        report.AverageBookingRevenue.Should().Be(2500m);
    }

    [Fact]
    public async Task GetBookingSummaryReportHandler_WithNoBookings_ShouldReturnZeros()
    {
        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);

        var report = await _summaryHandler.Handle(
            new GetBookingSummaryReportQuery(from, to), CancellationToken.None);

        report.TotalBookings.Should().Be(0);
        report.TotalRevenue.Should().Be(0);
        report.AverageBookingDurationHours.Should().Be(0);
        report.AverageBookingRevenue.Should().Be(0);
        report.PopularTimeSlots.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBookingSummaryReportHandler_ShouldReturnPopularTimeSlots()
    {
        var hall = SeedHall("Hall A", 50, 1000m);
        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);

        SeedBooking(hall.Id, from.AddDays(1).AddHours(10), from.AddDays(1).AddHours(11), 1000m);
        SeedBooking(hall.Id, from.AddDays(2).AddHours(10), from.AddDays(2).AddHours(11), 1000m);
        SeedBooking(hall.Id, from.AddDays(3).AddHours(10), from.AddDays(3).AddHours(11), 1000m);
        SeedBooking(hall.Id, from.AddDays(4).AddHours(14), from.AddDays(4).AddHours(15), 1000m);

        var report = await _summaryHandler.Handle(
            new GetBookingSummaryReportQuery(from, to), CancellationToken.None);

        report.PopularTimeSlots.First().Hour.Should().Be(10);
        report.PopularTimeSlots.First().BookingCount.Should().Be(3);
    }

    #endregion

    private Hall SeedHall(string name, int capacity, decimal rate)
    {
        var hall = new Hall(name, capacity, rate);
        _context.Halls.Add(hall);
        _context.SaveChanges();
        return hall;
    }

    private void SeedBooking(Guid hallId, DateTimeOffset start, DateTimeOffset end, decimal price)
    {
        var booking = new Booking(hallId, start, end, price);
        _context.Bookings.Add(booking);
        _context.SaveChanges();
    }
}
