using System.Net;
using System.Net.Http.Json;
using ConferenceHallBooking.Application.DTOs.Halls;
using ConferenceHallBooking.Application.DTOs.Options;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceHallBooking.UnitTests.Api;

public class HallApiTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public HallApiTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;

    #region GET /api/hall/{id}

    [Fact]
    public async Task GetById_WhenHallExists_ShouldReturn200WithHall()
    {
        var optionResponse = await _client.PostAsJsonAsync("/api/option", new { Name = "Projector", Price = 500m });
        var option = await optionResponse.Content.ReadFromJsonAsync<OptionResponse>();

        var createResponse = await _client.PostAsJsonAsync("/api/hall", new
        {
            Name = "Test Hall",
            Capacity = 50,
            BaseHourlyRate = 1000m,
            OptionIds = new[] { option!.Id }
        });
        var created = await createResponse.Content.ReadFromJsonAsync<HallResponse>();

        var response = await _client.GetAsync($"/api/hall/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var hall = await response.Content.ReadFromJsonAsync<HallResponse>();
        hall.Should().NotBeNull();
        hall!.Name.Should().Be("Test Hall");
        hall.Capacity.Should().Be(50);
        hall.BaseHourlyRate.Should().Be(1000m);
        hall.Options.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetById_WhenHallDoesNotExist_ShouldReturn404()
    {
        var response = await _client.GetAsync($"/api/hall/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_WithEmptyGuid_ShouldReturn400()
    {
        var response = await _client.GetAsync("/api/hall/00000000-0000-0000-0000-000000000000");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region POST /api/hall

    [Fact]
    public async Task Create_WithValidData_ShouldReturn201WithLocation()
    {
        var optionResponse1 = await _client.PostAsJsonAsync("/api/option", new { Name = "Projector", Price = 500m });
        var option1 = await optionResponse1.Content.ReadFromJsonAsync<OptionResponse>();
        var optionResponse2 = await _client.PostAsJsonAsync("/api/option", new { Name = "Wi-Fi", Price = 300m });
        var option2 = await optionResponse2.Content.ReadFromJsonAsync<OptionResponse>();

        var command = new
        {
            Name = "New Conference Hall",
            Capacity = 100,
            BaseHourlyRate = 2500m,
            OptionIds = new[] { option1!.Id, option2!.Id }
        };

        var response = await _client.PostAsJsonAsync("/api/hall", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var hall = await response.Content.ReadFromJsonAsync<HallResponse>();
        hall.Should().NotBeNull();
        hall!.Name.Should().Be("New Conference Hall");
        hall.Capacity.Should().Be(100);
        hall.BaseHourlyRate.Should().Be(2500m);
        hall.Options.Should().HaveCount(2);
    }

    [Fact]
    public async Task Create_WithoutOptions_ShouldReturn201WithEmptyOptions()
    {
        var command = new
        {
            Name = "Simple Hall",
            Capacity = 30,
            BaseHourlyRate = 800m
        };

        var response = await _client.PostAsJsonAsync("/api/hall", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var hall = await response.Content.ReadFromJsonAsync<HallResponse>();
        hall!.Options.Should().BeEmpty();
    }

    [Fact]
    public async Task Create_WithDuplicateName_ShouldReturn409()
    {
        var command1 = new { Name = "Unique Hall", Capacity = 50, BaseHourlyRate = 1000m };
        await _client.PostAsJsonAsync("/api/hall", command1);

        var command2 = new { Name = "Unique Hall", Capacity = 30, BaseHourlyRate = 800m };
        var response = await _client.PostAsJsonAsync("/api/hall", command2);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_WithEmptyName_ShouldReturn400()
    {
        var command = new
        {
            Name = "",
            Capacity = 50,
            BaseHourlyRate = 1000m
        };

        var response = await _client.PostAsJsonAsync("/api/hall", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithZeroCapacity_ShouldReturn400()
    {
        var command = new
        {
            Name = "Hall",
            Capacity = 0,
            BaseHourlyRate = 1000m
        };

        var response = await _client.PostAsJsonAsync("/api/hall", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithNegativeRate_ShouldReturn400()
    {
        var command = new
        {
            Name = "Hall",
            Capacity = 50,
            BaseHourlyRate = -100m
        };

        var response = await _client.PostAsJsonAsync("/api/hall", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithNonExistentOptionIds_ShouldReturn404()
    {
        var command = new
        {
            Name = "Hall With Bad Options",
            Capacity = 50,
            BaseHourlyRate = 1000m,
            OptionIds = new[] { Guid.NewGuid() }
        };

        var response = await _client.PostAsJsonAsync("/api/hall", command);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithExistingOptionIds_ShouldLinkOptionsToHall()
    {
        var optionResponse = await _client.PostAsJsonAsync("/api/option", new { Name = "Brand New Service", Price = 999m });
        var option = await optionResponse.Content.ReadFromJsonAsync<OptionResponse>();

        var command = new
        {
            Name = "Hall With Existing Options",
            Capacity = 50,
            BaseHourlyRate = 1000m,
            OptionIds = new[] { option!.Id }
        };

        var response = await _client.PostAsJsonAsync("/api/hall", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var hall = await response.Content.ReadFromJsonAsync<HallResponse>();
        hall!.Options.Should().HaveCount(1);
        hall.Options.First().Name.Should().Be("Brand New Service");
        hall.Options.First().Price.Should().Be(999m);
    }

    #endregion

    #region PUT /api/hall/{id}

    [Fact]
    public async Task Update_WithValidData_ShouldReturn200WithUpdatedHall()
    {
        var optionResponse = await _client.PostAsJsonAsync("/api/option", new { Name = "Projector", Price = 500m });
        var option = await optionResponse.Content.ReadFromJsonAsync<OptionResponse>();

        var createResponse = await _client.PostAsJsonAsync("/api/hall", new
        {
            Name = "Old Name",
            Capacity = 50,
            BaseHourlyRate = 1000m
        });
        var created = await createResponse.Content.ReadFromJsonAsync<HallResponse>();

        var updateResponse = await _client.PutAsJsonAsync($"/api/hall/{created!.Id}", new
        {
            Name = "New Name",
            Capacity = 80,
            BaseHourlyRate = 1500m,
            OptionIds = new[] { option!.Id }
        });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var hall = await updateResponse.Content.ReadFromJsonAsync<HallResponse>();
        hall!.Name.Should().Be("New Name");
        hall.Capacity.Should().Be(80);
        hall.BaseHourlyRate.Should().Be(1500m);
        hall.Options.Should().HaveCount(1);
    }

    [Fact]
    public async Task Update_WithDuplicateName_ShouldReturn409()
    {
        await _client.PostAsJsonAsync("/api/hall", new
        {
            Name = "Hall Alpha",
            Capacity = 50,
            BaseHourlyRate = 1000m
        });

        var createResponse = await _client.PostAsJsonAsync("/api/hall", new
        {
            Name = "Hall Beta",
            Capacity = 50,
            BaseHourlyRate = 1000m
        });
        var created = await createResponse.Content.ReadFromJsonAsync<HallResponse>();

        var updateResponse = await _client.PutAsJsonAsync($"/api/hall/{created!.Id}", new
        {
            Name = "Hall Alpha",
            Capacity = 50,
            BaseHourlyRate = 1000m
        });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Update_WhenHallDoesNotExist_ShouldReturn404()
    {
        var response = await _client.PutAsJsonAsync($"/api/hall/{Guid.NewGuid()}", new
        {
            Name = "Name",
            Capacity = 50,
            BaseHourlyRate = 1000m
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithEmptyName_ShouldReturn400()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/hall", new
        {
            Name = "Hall",
            Capacity = 50,
            BaseHourlyRate = 1000m
        });
        var created = await createResponse.Content.ReadFromJsonAsync<HallResponse>();

        var response = await _client.PutAsJsonAsync($"/api/hall/{created!.Id}", new
        {
            Name = "",
            Capacity = 50,
            BaseHourlyRate = 1000m
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WithZeroCapacity_ShouldReturn400()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/hall", new
        {
            Name = "Hall",
            Capacity = 50,
            BaseHourlyRate = 1000m
        });
        var created = await createResponse.Content.ReadFromJsonAsync<HallResponse>();

        var response = await _client.PutAsJsonAsync($"/api/hall/{created!.Id}", new
        {
            Name = "Hall",
            Capacity = 0,
            BaseHourlyRate = 1000m
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WithNegativeRate_ShouldReturn400()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/hall", new
        {
            Name = "Hall",
            Capacity = 50,
            BaseHourlyRate = 1000m
        });
        var created = await createResponse.Content.ReadFromJsonAsync<HallResponse>();

        var response = await _client.PutAsJsonAsync($"/api/hall/{created!.Id}", new
        {
            Name = "Hall",
            Capacity = 50,
            BaseHourlyRate = -100m
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WithEmptyGuid_ShouldReturn400()
    {
        var response = await _client.PutAsJsonAsync("/api/hall/00000000-0000-0000-0000-000000000000", new
        {
            Name = "Hall",
            Capacity = 50,
            BaseHourlyRate = 1000m
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region DELETE /api/hall/{id}

    [Fact]
    public async Task Delete_WhenHallExists_ShouldReturn204()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/hall", new
        {
            Name = "To Delete",
            Capacity = 20,
            BaseHourlyRate = 500m
        });
        var created = await createResponse.Content.ReadFromJsonAsync<HallResponse>();

        var response = await _client.DeleteAsync($"/api/hall/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/hall/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WhenHallDoesNotExist_ShouldReturn404()
    {
        var response = await _client.DeleteAsync($"/api/hall/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithEmptyGuid_ShouldReturn400()
    {
        var response = await _client.DeleteAsync("/api/hall/00000000-0000-0000-0000-000000000000");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_WhenHallHasBookings_ShouldReturn409()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/hall", new
        {
            Name = "Hall With Bookings",
            Capacity = 20,
            BaseHourlyRate = 500m
        });
        var created = await createResponse.Content.ReadFromJsonAsync<HallResponse>();

        var startTime = DateTimeOffset.UtcNow.AddDays(100);
        await _client.PostAsJsonAsync("/api/booking", new
        {
            HallId = created!.Id,
            StartTime = startTime,
            DurationHours = 2m
        });

        var response = await _client.DeleteAsync($"/api/hall/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(409);
    }

    #endregion

    #region GET /api/hall/available

    [Fact]
    public async Task GetAvailable_WhenHallsExist_ShouldReturn200WithList()
    {
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var endTime = startTime.AddHours(3);

        var url = $"/api/hall/available?startTime={Uri.EscapeDataString(startTime.ToString("O"))}&endTime={Uri.EscapeDataString(endTime.ToString("O"))}&capacity=10";
        var response = await _client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var halls = await response.Content.ReadFromJsonAsync<List<HallResponse>>();
        halls.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAvailable_WithExcessiveCapacity_ShouldReturnEmptyList()
    {
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var endTime = startTime.AddHours(3);

        var url = $"/api/hall/available?startTime={Uri.EscapeDataString(startTime.ToString("O"))}&endTime={Uri.EscapeDataString(endTime.ToString("O"))}&capacity=99999";
        var response = await _client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var halls = await response.Content.ReadFromJsonAsync<List<HallResponse>>();
        halls.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAvailable_WithZeroCapacity_ShouldReturn400()
    {
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var endTime = startTime.AddHours(3);

        var url = $"/api/hall/available?startTime={Uri.EscapeDataString(startTime.ToString("O"))}&endTime={Uri.EscapeDataString(endTime.ToString("O"))}&capacity=0";
        var response = await _client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAvailable_WithPastStartTime_ShouldReturn400()
    {
        var startTime = DateTimeOffset.UtcNow.AddDays(-1);
        var endTime = startTime.AddHours(3);

        var url = $"/api/hall/available?startTime={Uri.EscapeDataString(startTime.ToString("O"))}&endTime={Uri.EscapeDataString(endTime.ToString("O"))}&capacity=10";
        var response = await _client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAvailable_WithEndTimeBeforeStartTime_ShouldReturn400()
    {
        var startTime = DateTimeOffset.UtcNow.AddDays(2);
        var endTime = DateTimeOffset.UtcNow.AddDays(1);

        var url = $"/api/hall/available?startTime={Uri.EscapeDataString(startTime.ToString("O"))}&endTime={Uri.EscapeDataString(endTime.ToString("O"))}&capacity=10";
        var response = await _client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
}
