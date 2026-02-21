namespace Moongate.Server.Http.Data;

/// <summary>
/// Represents an authenticated user returned by the HTTP authentication bridge.
/// </summary>
public sealed class MoongateHttpAuthenticatedUser
{
    public required string AccountId { get; init; }

    public required string Username { get; init; }

    public required string Role { get; init; }
}
