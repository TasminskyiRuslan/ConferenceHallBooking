using System.Net;
using System.Net.Http.Json;
using ConferenceHallBooking.Application.DTOs.Options;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceHallBooking.UnitTests.Api;

public class OptionApiTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OptionApiTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    #region GET /api/option

    [Fact]
    public async Task GetAll_WhenOptionsExist_ShouldReturn200WithList()
    {
        await _client.PostAsJsonAsync("/api/option", new { Name = "Projector", Price = 500m });
        await _client.PostAsJsonAsync("/api/option", new { Name = "Wi-Fi", Price = 300m });

        var response = await _client.GetAsync("/api/option");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var options = await response.Content.ReadFromJsonAsync<List<OptionResponse>>();
        options.Should().NotBeNull();
        options!.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    #endregion

    #region GET /api/option/{id}

    [Fact]
    public async Task GetById_WhenOptionExists_ShouldReturn200WithOption()
    {
        var optionName = $"Projector_{Guid.NewGuid():N}";
        var createResponse = await _client.PostAsJsonAsync("/api/option", new { Name = optionName, Price = 500m });
        var created = await createResponse.Content.ReadFromJsonAsync<OptionResponse>();

        var response = await _client.GetAsync($"/api/option/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var option = await response.Content.ReadFromJsonAsync<OptionResponse>();
        option.Should().NotBeNull();
        option!.Name.Should().Be(optionName);
        option.Price.Should().Be(500m);
    }

    [Fact]
    public async Task GetById_WhenOptionDoesNotExist_ShouldReturn404()
    {
        var response = await _client.GetAsync($"/api/option/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_WithEmptyGuid_ShouldReturn400()
    {
        var response = await _client.GetAsync("/api/option/00000000-0000-0000-0000-000000000000");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region POST /api/option

    [Fact]
    public async Task Create_WithValidData_ShouldReturn201WithOption()
    {
        var command = new { Name = $"Projector_{Guid.NewGuid():N}", Price = 500m };

        var response = await _client.PostAsJsonAsync("/api/option", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var option = await response.Content.ReadFromJsonAsync<OptionResponse>();
        option.Should().NotBeNull();
        option!.Name.Should().Be(command.Name);
        option.Price.Should().Be(500m);
    }

    [Fact]
    public async Task Create_WithEmptyName_ShouldReturn400()
    {
        var command = new { Name = "", Price = 500m };

        var response = await _client.PostAsJsonAsync("/api/option", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithZeroPrice_ShouldReturn201AsFreeOption()
    {
        var command = new { Name = "Free Service", Price = 0m };

        var response = await _client.PostAsJsonAsync("/api/option", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var option = await response.Content.ReadFromJsonAsync<OptionResponse>();
        option!.Price.Should().Be(0m);
    }

    [Fact]
    public async Task Create_WithNegativePrice_ShouldReturn400()
    {
        var command = new { Name = "Projector", Price = -100m };

        var response = await _client.PostAsJsonAsync("/api/option", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region PUT /api/option/{id}

    [Fact]
    public async Task Update_WhenOptionExists_ShouldReturn200WithUpdatedOption()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/option", new { Name = $"Projector_{Guid.NewGuid():N}", Price = 500m });
        var created = await createResponse.Content.ReadFromJsonAsync<OptionResponse>();

        var updateResponse = await _client.PutAsJsonAsync($"/api/option/{created!.Id}", new { Name = "HD Projector", Price = 750m });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var option = await updateResponse.Content.ReadFromJsonAsync<OptionResponse>();
        option!.Name.Should().Be("HD Projector");
        option.Price.Should().Be(750m);
    }

    [Fact]
    public async Task Update_WhenOptionDoesNotExist_ShouldReturn404()
    {
        var response = await _client.PutAsJsonAsync($"/api/option/{Guid.NewGuid()}", new { Name = "Projector", Price = 500m });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithEmptyName_ShouldReturn400()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/option", new { Name = $"Projector_{Guid.NewGuid():N}", Price = 500m });
        var created = await createResponse.Content.ReadFromJsonAsync<OptionResponse>();

        var response = await _client.PutAsJsonAsync($"/api/option/{created!.Id}", new { Name = "", Price = 500m });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region DELETE /api/option/{id}

    [Fact]
    public async Task Delete_WhenOptionExists_ShouldReturn204()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/option", new { Name = $"Projector_{Guid.NewGuid():N}", Price = 500m });
        var created = await createResponse.Content.ReadFromJsonAsync<OptionResponse>();

        var response = await _client.DeleteAsync($"/api/option/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/option/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WhenOptionDoesNotExist_ShouldReturn404()
    {
        var response = await _client.DeleteAsync($"/api/option/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
