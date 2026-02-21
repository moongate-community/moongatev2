namespace Moongate.Server.Data.Config;

public class MoongateHttpJwtConfig
{
    public bool IsEnabled { get; set; }

    public string SigningKey { get; set; } = string.Empty;

    public string Issuer { get; set; } = "moongate-http";

    public string Audience { get; set; } = "moongate-http-client";

    public int ExpirationMinutes { get; set; } = 60;
}
