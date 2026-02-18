using System.Text.Json.Serialization;
using Moongate.Core.Types;

namespace Moongate.Server.Data.Config;

public class MoongateConfig
{
    [JsonIgnore]
    public string RootDirectory { get; set; }

    public string UODirectory { get; set; }

    public LogLevelType LogLevel { get; set; }

    public bool LogPacketData { get; set; }

    public bool IsDeveloperMode { get; set; }

    public MoongateHttpConfig Http { get; set; } = new();

    public MoongateGameConfig Game { get; set; } = new();
}
