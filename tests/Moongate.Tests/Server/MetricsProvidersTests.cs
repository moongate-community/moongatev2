using Moongate.Server.Services.Metrics.Providers;
using Moongate.Tests.Server.Support;

namespace Moongate.Tests.Server;

public class MetricsProvidersTests
{
    private static readonly string[] GameLoopMetricNames =
    [
        "loop.tick.duration.avg_ms",
        "loop.tick.duration.max_ms",
        "loop.idle.sleep.count",
        "loop.work.units.avg",
        "network.outbound.queue.depth",
        "network.outbound.packets.total",
        "ticks.total",
        "tick.duration.avg_ms",
        "uptime.ms"
    ];

    private static readonly string[] NetworkMetricNames =
    [
        "network.inbound.queue.depth",
        "network.inbound.packets.total",
        "network.inbound.unknown_opcode.total",
        "sessions.active",
        "packets.parsed.total",
        "bytes.received.total",
        "parser.errors.total"
    ];

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
        "persistence.save.duration.last_ms",
        "snapshot.saves.total",
        "snapshot.save.duration.last_ms",
        "snapshot.save.timestamp_utc_ms",
        "snapshot.save.errors.total"
    ];

    private static readonly string[] TimerMetricNames =
    [
        "timer.processed_ticks.total",
        "active.count",
        "registered.total",
        "callbacks.executed.total",
        "callbacks.errors.total",
        "callback.duration.avg_ms"
    ];

    [Test]
    public async Task GameLoopMetricsProvider_ShouldExposeExpectedMetricNames()
    {
        var provider = new GameLoopMetricsProvider(
            new MetricsProvidersTestGameLoopService
            {
                TickCount = 10,
                AverageTickMs = 15.5,
                MaxTickMs = 20.2,
                IdleSleepCount = 12,
                AverageWorkUnits = 4.3,
                OutboundQueueDepth = 7,
                OutboundPacketsTotal = 99,
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
                TotalParserErrors = 3,
                InboundQueueDepth = 6,
                TotalUnknownOpcodeDrops = 2
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

    [Test]
    public async Task TimerMetricsProvider_ShouldExposeExpectedMetricNames()
    {
        var provider = new TimerMetricsProvider(
            new MetricsProvidersTestTimerService
            {
                ActiveTimerCount = 5,
                TotalRegisteredTimers = 12,
                TotalExecutedCallbacks = 10,
                CallbackErrors = 1,
                AverageCallbackDurationMs = 0.5,
                TotalProcessedTicks = 1200
            }
        );

        var samples = await provider.CollectAsync();
        var names = samples.Select(sample => sample.Name).ToArray();

        Assert.That(names, Is.EquivalentTo(TimerMetricNames));
    }
}
