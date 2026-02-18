namespace Moongate.Server.Data.Config;

public class MoongateHttpConfig
{
    public bool IsHttpEnabled { get; set; } = true;

    public int Port { get; set; } = 8088;

    public bool IsOpenApiEnabled { get; set; } = true;
}
