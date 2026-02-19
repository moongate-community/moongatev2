using Moongate.Server.Http.Data;

namespace Moongate.Server.Interfaces.Services.Metrics;

/// <summary>
/// Builds HTTP-facing metrics snapshots from the internal metrics collection state.
/// </summary>
public interface IMetricsHttpSnapshotFactory
{
    /// <summary>
    /// Creates the latest HTTP-facing metrics snapshot.
    /// </summary>
    /// <returns>A snapshot ready for HTTP exposure.</returns>
    MoongateHttpMetricsSnapshot? CreateSnapshot();
}
