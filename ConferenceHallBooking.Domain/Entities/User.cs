using ConferenceHallBooking.Domain.Exceptions;

namespace ConferenceHallBooking.Domain.Entities;

/// <summary>
/// Represents a system user with authentication credentials and role.
/// </summary>
public class User
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; private set; }

    /// <summary>Email address used for login (unique).</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>BCrypt-hashed password.</summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>Display name of the user.</summary>
    public string FullName { get; private set; } = string.Empty;

    /// <summary>Authorization role (e.g., "User", "Admin").</summary>
    public string Role { get; private set; } = "User";

    /// <summary>UTC timestamp when the user was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private User() { }

    public User(string email, string passwordHash, string fullName, string role = "User")
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidEntityFieldException(nameof(User), nameof(Email), "email cannot be empty");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new InvalidEntityFieldException(nameof(User), nameof(PasswordHash), "password hash cannot be empty");

        if (string.IsNullOrWhiteSpace(fullName))
            throw new InvalidEntityFieldException(nameof(User), nameof(FullName), "full name cannot be empty");

        Id = Guid.NewGuid();
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        Role = role;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }
}
