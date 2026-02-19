using Moongate.Core.Types;

namespace Moongate.Server.Data.Config;

/// <summary>
/// Configures server metrics collection cadence and logging behavior.
/// </summary>
public class MoongateMetricsConfig
{
    public bool Enabled { get; set; } = true;

    public int IntervalMilliseconds { get; set; } = 1000;

    public bool LogEnabled { get; set; } = true;

    public bool LogToConsole { get; set; }

    public LogLevelType LogLevel { get; set; } = LogLevelType.Trace;
}
