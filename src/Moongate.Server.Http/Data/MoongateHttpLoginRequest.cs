namespace Moongate.Server.Http.Data;

/// <summary>
/// Login request payload for JWT authentication.
/// </summary>
public sealed class MoongateHttpLoginRequest
{
    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
