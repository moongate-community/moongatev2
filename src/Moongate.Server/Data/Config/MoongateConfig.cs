using Moongate.Core.Types;

namespace Moongate.Server.Data.Config;

public class MoongateConfig
{
    public string RootDirectory { get; set; }

    public LogLevelType LogLevel { get; set; }

    public bool LogPacketData { get; set; }

}
