using Moongate.Server.Data.Metrics;
using Moongate.Server.Interfaces.Services.Metrics;

namespace Moongate.Server.Services.Metrics.Providers;

/// <summary>
/// Exposes network and parser metrics.
/// </summary>
public sealed class NetworkMetricsProvider : IMetricProvider
{
    private readonly INetworkMetricsSource _networkMetricsSource;

    public NetworkMetricsProvider(INetworkMetricsSource networkMetricsSource)
        => _networkMetricsSource = networkMetricsSource;

    public string ProviderName => "network";

    public ValueTask<IReadOnlyList<MetricSample>> CollectAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = _networkMetricsSource.GetMetricsSnapshot();

        return ValueTask.FromResult<IReadOnlyList<MetricSample>>(
            [
                new("network.inbound.queue.depth", snapshot.InboundQueueDepth),
                new("network.inbound.packets.total", snapshot.TotalParsedPackets),
                new("network.inbound.unknown_opcode.total", snapshot.TotalUnknownOpcodeDrops),
                new("sessions.active", snapshot.ActiveSessionCount),
                new("packets.parsed.total", snapshot.TotalParsedPackets),
                new("bytes.received.total", snapshot.TotalReceivedBytes),
                new("parser.errors.total", snapshot.TotalParserErrors)
            ]
        );
    }
}
