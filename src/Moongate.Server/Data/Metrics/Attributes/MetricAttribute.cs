using Moongate.Server.Data.Metrics.Types;

namespace Moongate.Server.Data.Metrics.Attributes;

/// <summary>
/// Declares a metric sample mapping for a snapshot property.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class MetricAttribute : Attribute
{
    public MetricAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets the metric name emitted for the annotated property.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets optional alias metric names emitted with the same value.
    /// </summary>
    public string[] Aliases { get; init; } = [];

    /// <summary>
    /// Gets or sets the value transformation applied before emitting the metric sample.
    /// </summary>
    public MetricValueTransformType Transform { get; init; } = MetricValueTransformType.None;
}
