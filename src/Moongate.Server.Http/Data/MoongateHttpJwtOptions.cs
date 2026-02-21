namespace Moongate.Server.Http.Data;

/// <summary>
/// Configuration for JWT token generation and validation.
/// </summary>
public sealed class MoongateHttpJwtOptions
{
    public bool IsEnabled { get; init; } = false;

    public string SigningKey { get; init; } = string.Empty;

    public string Issuer { get; init; } = "moongate-http";

    public string Audience { get; init; } = "moongate-http-client";

    public int ExpirationMinutes { get; init; } = 60;
}
