using Moongate.Server.Data.Metrics;
using Moongate.Server.Data.Config;
using Moongate.Server.Interfaces.Services.Metrics;
using Moongate.Server.Services.Metrics;
using Moongate.Tests.Server.Support;

namespace Moongate.Tests.Server;

public class MetricsCollectionServiceTests
{
    [Test]
    public async Task StartAsync_ShouldCollectMetricsOnInterval()
    {
        var provider = new MetricsCollectionCountingProvider("test", "ticks.total", 7);
        var service = new MetricsCollectionService(
            [provider],
            new MoongateMetricsConfig { IntervalMilliseconds = 25, LogEnabled = false }
        );

        await service.StartAsync();
        await Task.Delay(90);
        await service.StopAsync();

        var metrics = service.GetAllMetrics();

        Assert.That(metrics.TryGetValue("test.ticks.total", out var sample), Is.True);
        Assert.That(sample!.Value, Is.EqualTo(7d));
    }

    [Test]
    public async Task StartAsync_WhenOneProviderThrows_ShouldContinueCollectingOtherProviders()
    {
        var provider = new MetricsCollectionCountingProvider("ok", "packets.total", 42);
        var service = new MetricsCollectionService(
            [new MetricsCollectionThrowingProvider("bad"), provider],
            new MoongateMetricsConfig { IntervalMilliseconds = 25, LogEnabled = false }
        );

        await service.StartAsync();
        await Task.Delay(90);
        await service.StopAsync();

        var metrics = service.GetAllMetrics();

        Assert.That(metrics.TryGetValue("ok.packets.total", out var sample), Is.True);
        Assert.That(sample!.Value, Is.EqualTo(42d));
    }

    [Test]
    public void MetricSample_ShouldStoreProvidedNameAndValue()
    {
        var sample = new MetricSample("gameloop.ticks.total", 12);

        Assert.Multiple(
            () =>
            {
                Assert.That(sample.Name, Is.EqualTo("gameloop.ticks.total"));
                Assert.That(sample.Value, Is.EqualTo(12d));
            }
        );
    }

    [Test]
    public void MetricsSnapshot_ShouldExposeCollectedMetrics()
    {
        var samples = new Dictionary<string, MetricSample>
        {
            ["gameloop.ticks.total"] = new("gameloop.ticks.total", 99)
        };

        var snapshot = new MetricsSnapshot(DateTimeOffset.UtcNow, samples);

        Assert.That(snapshot.Metrics["gameloop.ticks.total"].Value, Is.EqualTo(99d));
    }

    [Test]
    public void IMetricProvider_Contract_ShouldExposeProviderName()
        => Assert.That(new MetricsCollectionCountingProvider("test", "x", 0).ProviderName, Is.EqualTo("test"));
}
