using System.Net;
using System.Net.Http.Json;
using ConferenceHallBooking.Application.DTOs.Bookings;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceHallBooking.UnitTests.Api;

public class BookingApiTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;
    private Guid _hallId;
    private Guid _optionId;
    private int _dayOffset = 10;

    public BookingApiTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var option = new Option("Projector", 500m);
        context.Options.Add(option);

        var hall = new Hall("Test Hall", 50, 1000m);
        hall.AddOption(option.Id);

        context.Halls.Add(hall);
        await context.SaveChangesAsync();

        _hallId = hall.Id;
        _optionId = option.Id;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private DateTimeOffset NextStartTime()
    {
        var offset = Interlocked.Increment(ref _dayOffset);
        return DateTimeOffset.UtcNow.AddDays(offset);
    }

    #region POST /api/booking

    [Fact]
    public async Task Create_WithValidData_ShouldReturn201WithBooking()
    {
        var startTime = NextStartTime();
        var command = new
        {
            HallId = _hallId,
            StartTime = startTime,
            DurationHours = 2m,
            OptionIds = Array.Empty<Guid>()
        };

        var response = await _client.PostAsJsonAsync("/api/booking", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var booking = await response.Content.ReadFromJsonAsync<BookingResponse>();
        booking.Should().NotBeNull();
        booking!.HallId.Should().Be(_hallId);
        booking.DurationHours.Should().Be(2m);
    }

    [Fact]
    public async Task Create_WithOptions_ShouldReturn201WithSelectedOptions()
    {
        var startTime = NextStartTime();
        var command = new
        {
            HallId = _hallId,
            StartTime = startTime,
            DurationHours = 3m,
            OptionIds = new[] { _optionId }
        };

        var response = await _client.PostAsJsonAsync("/api/booking", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var booking = await response.Content.ReadFromJsonAsync<BookingResponse>();
        booking!.SelectedOptions.Should().HaveCount(1);
    }

    [Fact]
    public async Task Create_WithZeroDuration_ShouldReturn400()
    {
        var command = new
        {
            HallId = _hallId,
            StartTime = NextStartTime(),
            DurationHours = 0m,
            OptionIds = Array.Empty<Guid>()
        };

        var response = await _client.PostAsJsonAsync("/api/booking", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(400);
    }

    [Fact]
    public async Task Create_WithNegativeDuration_ShouldReturn400()
    {
        var command = new
        {
            HallId = _hallId,
            StartTime = NextStartTime(),
            DurationHours = -2m,
            OptionIds = Array.Empty<Guid>()
        };

        var response = await _client.PostAsJsonAsync("/api/booking", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithNonExistentHall_ShouldReturn404()
    {
        var command = new
        {
            HallId = Guid.NewGuid(),
            StartTime = NextStartTime(),
            DurationHours = 2m,
            OptionIds = Array.Empty<Guid>()
        };

        var response = await _client.PostAsJsonAsync("/api/booking", command);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(404);
    }

    [Fact]
    public async Task Create_WhenTimeSlotOverlaps_ShouldReturn409()
    {
        var startTime = NextStartTime();

        var first = new
        {
            HallId = _hallId,
            StartTime = startTime,
            DurationHours = 3m,
            OptionIds = Array.Empty<Guid>()
        };
        await _client.PostAsJsonAsync("/api/booking", first);

        var overlapping = new
        {
            HallId = _hallId,
            StartTime = startTime.AddHours(1),
            DurationHours = 2m,
            OptionIds = Array.Empty<Guid>()
        };

        var response = await _client.PostAsJsonAsync("/api/booking", overlapping);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(409);
    }

    [Fact]
    public async Task Create_WithUnsupportedOption_ShouldReturn409()
    {
        var command = new
        {
            HallId = _hallId,
            StartTime = NextStartTime(),
            DurationHours = 2m,
            OptionIds = new[] { Guid.NewGuid() }
        };

        var response = await _client.PostAsJsonAsync("/api/booking", command);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_WithPastStartTime_ShouldReturn400()
    {
        var command = new
        {
            HallId = _hallId,
            StartTime = DateTimeOffset.UtcNow.AddDays(-1),
            DurationHours = 2m,
            OptionIds = Array.Empty<Guid>()
        };

        var response = await _client.PostAsJsonAsync("/api/booking", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithEmptyHallId_ShouldReturn400()
    {
        var command = new
        {
            HallId = Guid.Empty,
            StartTime = NextStartTime(),
            DurationHours = 2m,
            OptionIds = Array.Empty<Guid>()
        };

        var response = await _client.PostAsJsonAsync("/api/booking", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GET /api/booking/{id}

    [Fact]
    public async Task GetById_WhenBookingExists_ShouldReturn200WithBooking()
    {
        var startTime = NextStartTime();
        var createResponse = await _client.PostAsJsonAsync("/api/booking", new
        {
            HallId = _hallId,
            StartTime = startTime,
            DurationHours = 2m,
            OptionIds = Array.Empty<Guid>()
        });
        var created = await createResponse.Content.ReadFromJsonAsync<BookingResponse>();

        var response = await _client.GetAsync($"/api/booking/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var booking = await response.Content.ReadFromJsonAsync<BookingResponse>();
        booking.Should().NotBeNull();
        booking!.HallId.Should().Be(_hallId);
    }

    [Fact]
    public async Task GetById_WhenBookingDoesNotExist_ShouldReturn404()
    {
        var response = await _client.GetAsync($"/api/booking/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_WithEmptyGuid_ShouldReturn400()
    {
        var response = await _client.GetAsync("/api/booking/00000000-0000-0000-0000-000000000000");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
}
