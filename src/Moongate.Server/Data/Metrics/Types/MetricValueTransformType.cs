namespace Moongate.Server.Data.Metrics.Types;

/// <summary>
/// Controls how a metric property value is transformed into a numeric metric sample value.
/// </summary>
public enum MetricValueTransformType
{
    None = 0,
    TimeSpanMilliseconds = 1,
    UnixTimeMillisecondsOrZero = 2
}
