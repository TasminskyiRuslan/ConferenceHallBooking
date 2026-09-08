using ConferenceHallBooking.Application.DTOs.Auth;
using ConferenceHallBooking.Application.Features.Auth.Commands;
using ConferenceHallBooking.Application.Features.Auth.Handlers;
using ConferenceHallBooking.Application.Interfaces.Auth;
using FluentAssertions;
using NSubstitute;

namespace ConferenceHallBooking.UnitTests.Application.Features.Auth;

public class AuthHandlerTests
{
    private readonly IAuthService _authService = Substitute.For<IAuthService>();

    #region RegisterCommandHandler

    [Fact]
    public async Task RegisterCommandHandler_WithValidData_ShouldReturnAuthResponse()
    {
        var handler = new RegisterCommandHandler(_authService);
        var command = new RegisterCommand("user@email.com", "password123", "password123", "John Doe");

        var expected = new AuthResponse(Guid.NewGuid(), "user@email.com", "John Doe", "User", "jwt-token");
        _authService.RegisterAsync(command, Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().Be(expected);
        result.Email.Should().Be("user@email.com");
        result.Token.Should().Be("jwt-token");
        await _authService.Received(1).RegisterAsync(command, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterCommandHandler_WithDuplicateEmail_ShouldPropagateException()
    {
        var handler = new RegisterCommandHandler(_authService);
        var command = new RegisterCommand("existing@email.com", "password123", "password123", "John");

        _authService.RegisterAsync(command, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<AuthResponse>(
                new ConferenceHallBooking.Domain.Exceptions.EmailAlreadyExistsException("existing@email.com")));

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConferenceHallBooking.Domain.Exceptions.EmailAlreadyExistsException>();
    }

    #endregion

    #region LoginCommandHandler

    [Fact]
    public async Task LoginCommandHandler_WithValidCredentials_ShouldReturnAuthResponse()
    {
        var handler = new LoginCommandHandler(_authService);
        var command = new LoginCommand("user@email.com", "password123");

        var expected = new AuthResponse(Guid.NewGuid(), "user@email.com", "John Doe", "User", "jwt-token");
        _authService.LoginAsync(command, Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().Be(expected);
        result.Email.Should().Be("user@email.com");
        result.Token.Should().Be("jwt-token");
        await _authService.Received(1).LoginAsync(command, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LoginCommandHandler_WithInvalidCredentials_ShouldPropagateException()
    {
        var handler = new LoginCommandHandler(_authService);
        var command = new LoginCommand("user@email.com", "wrongpassword");

        _authService.LoginAsync(command, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<AuthResponse>(
                new ConferenceHallBooking.Domain.Exceptions.InvalidCredentialsException()));

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConferenceHallBooking.Domain.Exceptions.InvalidCredentialsException>();
    }

    #endregion
}
