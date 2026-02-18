using Moongate.Network.Pipeline;
using Moongate.Tests.Network.Support;

namespace Moongate.Tests.Network;

public class MiddlewarePipelineTests
{
    [Test]
    public async Task ExecuteAsync_ShouldProcessMiddlewaresInOrder()
    {
        var pipeline = new NetMiddlewarePipeline(
            [
                new MiddlewarePipelinePrefixMiddleware("A"),
                new MiddlewarePipelinePrefixMiddleware("B")
            ]
        );

        var input = "payload"u8.ToArray();
        var output = await pipeline.ExecuteAsync(null, input, CancellationToken.None);

        Assert.That(output.ToArray(), Is.EqualTo("BApayload"u8.ToArray()));
    }

    [Test]
    public async Task ExecuteAsync_WhenMiddlewareReturnsEmpty_ShouldStopPipeline()
    {
        var pipeline = new NetMiddlewarePipeline(
            [
                new MiddlewarePipelineEmptyMiddleware(),
                new MiddlewarePipelinePrefixMiddleware("X")
            ]
        );

        var input = "payload"u8.ToArray();
        var output = await pipeline.ExecuteAsync(null, input, CancellationToken.None);

        Assert.That(output.IsEmpty, Is.True);
    }

    [Test]
    public async Task ExecuteSendAsync_ShouldProcessMiddlewaresInOrder()
    {
        var pipeline = new NetMiddlewarePipeline(
            [
                new MiddlewarePipelineSendPrefixMiddleware("A"),
                new MiddlewarePipelineSendPrefixMiddleware("B")
            ]
        );

        var input = "payload"u8.ToArray();
        var output = await pipeline.ExecuteSendAsync(null, input, CancellationToken.None);

        Assert.That(output.ToArray(), Is.EqualTo("BApayload"u8.ToArray()));
    }
}
