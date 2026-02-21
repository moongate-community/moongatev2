namespace Moongate.Server.Http.Data;

/// <summary>
/// Login response payload containing the generated JWT token.
/// </summary>
public sealed class MoongateHttpLoginResponse
{
    public required string AccessToken { get; init; }

    public required string TokenType { get; init; }

    public required DateTimeOffset ExpiresAtUtc { get; init; }

    public required string AccountId { get; init; }

    public required string Username { get; init; }

    public required string Role { get; init; }
}
