using Moongate.Core.Types;
using Serilog.Events;

namespace Moongate.Core.Extensions.Logger;

/// <summary>
/// Extension methods for converting log levels between different logging frameworks.
/// </summary>
public static class LogLevelExtensions
{
    /// <summary>
    /// Converts a LogLevelType to a Serilog LogEventLevel.
    /// </summary>
    /// <param name="logLevel">The log level to convert.</param>
    /// <returns>The corresponding Serilog log event level.</returns>
    public static LogEventLevel ToSerilogLogLevel(this LogLevelType logLevel)
    {
        return logLevel switch
        {
            LogLevelType.Trace       => LogEventLevel.Verbose,
            LogLevelType.Debug       => LogEventLevel.Debug,
            LogLevelType.Information => LogEventLevel.Information,
            LogLevelType.Warning     => LogEventLevel.Warning,
            LogLevelType.Error       => LogEventLevel.Error,
            _                        => LogEventLevel.Information
        };
    }
}
