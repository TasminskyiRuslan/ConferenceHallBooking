using System.Net;
using System.Net.Http.Json;
using ConferenceHallBooking.Application.DTOs.Auth;
using FluentAssertions;

namespace ConferenceHallBooking.UnitTests.Api;

public class AuthApiTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthApiTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturn201WithToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "newuser@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FullName = "Test User"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.Email.Should().Be("newuser@test.com");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturn409()
    {
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "duplicate@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FullName = "First User"
        });

        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "duplicate@test.com",
            Password = "Password456!",
            ConfirmPassword = "Password456!",
            FullName = "Second User"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithEmptyEmail_ShouldReturn400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FullName = "Test User"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithMismatchedPasswords_ShouldReturn400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "user@test.com",
            Password = "Password123!",
            ConfirmPassword = "DifferentPassword!",
            FullName = "Test User"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturn200WithToken()
    {
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "loginuser@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FullName = "Login User"
        });

        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "loginuser@test.com",
            Password = "Password123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturn401()
    {
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "wrongpw@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FullName = "User"
        });

        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "wrongpw@test.com",
            Password = "WrongPassword!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ShouldReturn401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "nonexistent@test.com",
            Password = "Password123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
