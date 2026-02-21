namespace Moongate.Server.Metrics.Data;

/// <summary>
/// Defines the type of a Prometheus metric.
/// </summary>
public enum MetricType
{
    /// <summary>
    /// A cumulative counter that only increases (e.g., total requests processed).
    /// </summary>
    Counter,

    /// <summary>
    /// A gauge that can go up or down (e.g., current connections).
    /// </summary>
    Gauge,

    /// <summary>
    /// A histogram for measuring distributions (e.g., request latency).
    /// </summary>
    Histogram,
}
