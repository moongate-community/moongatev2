using Moongate.Server.Data.Metrics;
using Moongate.Server.Interfaces.Services.Metrics;

namespace Moongate.Tests.Server.Support;

public sealed class MetricsProvidersTestNetworkService : INetworkMetricsSource
{
    public int ActiveSessionCount { get; set; }

    public long TotalReceivedBytes { get; set; }

    public int TotalParsedPackets { get; set; }

    public int TotalParserErrors { get; set; }

    public NetworkMetricsSnapshot GetMetricsSnapshot()
        => new(ActiveSessionCount, TotalReceivedBytes, TotalParsedPackets, TotalParserErrors);
}
