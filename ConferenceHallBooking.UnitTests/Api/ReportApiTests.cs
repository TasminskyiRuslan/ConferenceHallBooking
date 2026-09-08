using System.Net;
using System.Net.Http.Json;
using ConferenceHallBooking.Application.DTOs.Reports;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Infrastructure.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceHallBooking.UnitTests.Api;

public class ReportApiTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;
    private Guid _hallId;
    private DateTimeOffset _from;
    private DateTimeOffset _to;

    public ReportApiTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        context.Bookings.RemoveRange(context.Bookings);
        context.Halls.RemoveRange(context.Halls);
        await context.SaveChangesAsync();

        var hall = new Hall("Test Hall", 50, 1000m);
        context.Halls.Add(hall);
        await context.SaveChangesAsync();
        _hallId = hall.Id;

        _from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        _to = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);

        var booking = new Booking(
            _hallId,
            _from.AddDays(5).AddHours(10),
            _from.AddDays(5).AddHours(14),
            4000m);
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetRevenue_WithValidRange_ShouldReturn200()
    {
        var response = await _client.GetAsync(
            $"/api/report/revenue?from={Uri.EscapeDataString(_from.ToString("O"))}&to={Uri.EscapeDataString(_to.ToString("O"))}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var report = await response.Content.ReadFromJsonAsync<RevenueReport>();
        report.Should().NotBeNull();
        report!.TotalRevenue.Should().Be(4000m);
    }

    [Fact]
    public async Task GetUtilization_WithValidRange_ShouldReturn200()
    {
        var response = await _client.GetAsync(
            $"/api/report/utilization?from={Uri.EscapeDataString(_from.ToString("O"))}&to={Uri.EscapeDataString(_to.ToString("O"))}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var report = await response.Content.ReadFromJsonAsync<HallUtilizationReport>();
        report.Should().NotBeNull();
        report!.Halls.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetSummary_WithValidRange_ShouldReturn200()
    {
        var response = await _client.GetAsync(
            $"/api/report/summary?from={Uri.EscapeDataString(_from.ToString("O"))}&to={Uri.EscapeDataString(_to.ToString("O"))}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var report = await response.Content.ReadFromJsonAsync<BookingSummaryReport>();
        report.Should().NotBeNull();
        report!.TotalBookings.Should().Be(1);
    }

    [Fact]
    public async Task GetRevenue_WithEndBeforeStart_ShouldReturn400()
    {
        var response = await _client.GetAsync(
            $"/api/report/revenue?from={Uri.EscapeDataString(_to.ToString("O"))}&to={Uri.EscapeDataString(_from.ToString("O"))}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
