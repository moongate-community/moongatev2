using Moongate.Server.Services.Metrics.Providers;
using Moongate.Tests.Server.Support;

namespace Moongate.Tests.Server;

public class MetricsProvidersTests
{
    private static readonly string[] GameLoopMetricNames = ["ticks.total", "tick.duration.avg_ms", "uptime.ms"];

    private static readonly string[] NetworkMetricNames =
        ["sessions.active", "packets.parsed.total", "bytes.received.total", "parser.errors.total"];

    private static readonly string[] ScriptingMetricNames =
    [
        "execution.time_ms",
        "memory.used_bytes",
        "statements.executed",
        "cache.hits.total",
        "cache.misses.total",
        "cache.entries.total"
    ];

    private static readonly string[] PersistenceMetricNames =
    [
        "snapshot.saves.total",
        "snapshot.save.duration.last_ms",
        "snapshot.save.timestamp_utc_ms",
        "snapshot.save.errors.total"
    ];

    [Test]
    public async Task GameLoopMetricsProvider_ShouldExposeExpectedMetricNames()
    {
        var provider = new GameLoopMetricsProvider(
            new MetricsProvidersTestGameLoopService
            {
                TickCount = 10,
                AverageTickMs = 15.5,
                Uptime = TimeSpan.FromSeconds(5)
            }
        );

        var samples = await provider.CollectAsync();
        var names = samples.Select(sample => sample.Name).ToArray();

        Assert.That(names, Is.EquivalentTo(GameLoopMetricNames));
    }

    [Test]
    public async Task NetworkMetricsProvider_ShouldExposeExpectedMetricNames()
    {
        var provider = new NetworkMetricsProvider(
            new MetricsProvidersTestNetworkService
            {
                ActiveSessionCount = 2,
                TotalParsedPackets = 25,
                TotalReceivedBytes = 4096,
                TotalParserErrors = 3
            }
        );

        var samples = await provider.CollectAsync();
        var names = samples.Select(sample => sample.Name).ToArray();

        Assert.That(names, Is.EquivalentTo(NetworkMetricNames));
    }

    [Test]
    public async Task PersistenceMetricsProvider_ShouldExposeExpectedMetricNames()
    {
        var provider = new PersistenceMetricsProvider(
            new MetricsProvidersTestPersistenceService
            {
                TotalSaves = 4,
                LastSaveDurationMs = 12.5,
                LastSaveTimestampUtc = DateTimeOffset.UnixEpoch.AddSeconds(10),
                SaveErrors = 1
            }
        );

        var samples = await provider.CollectAsync();
        var names = samples.Select(sample => sample.Name).ToArray();

        Assert.That(names, Is.EquivalentTo(PersistenceMetricNames));
    }

    [Test]
    public async Task ScriptEngineMetricsProvider_ShouldExposeExpectedMetricNames()
    {
        var provider = new ScriptEngineMetricsProvider(new GameEventScriptBridgeTestScriptEngineService());

        var samples = await provider.CollectAsync();
        var names = samples.Select(sample => sample.Name).ToArray();

        Assert.That(names, Is.EquivalentTo(ScriptingMetricNames));
    }
}
