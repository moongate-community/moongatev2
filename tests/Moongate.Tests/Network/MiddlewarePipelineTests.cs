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

    [Test]
    public async Task AddMiddleware_ShouldAddMiddlewareToPipeline()
    {
        var pipeline = new NetMiddlewarePipeline();
        pipeline.AddMiddleware(new MiddlewarePipelinePrefixMiddleware("A"));
        pipeline.AddMiddleware(new MiddlewarePipelinePrefixMiddleware("B"));

        var output = await pipeline.ExecuteAsync(null, "payload"u8.ToArray(), CancellationToken.None);

        Assert.That(output.ToArray(), Is.EqualTo("BApayload"u8.ToArray()));
    }

    [Test]
    public async Task RemoveMiddleware_ShouldRemoveAllMatchingMiddlewares()
    {
        var pipeline = new NetMiddlewarePipeline(
            [
                new MiddlewarePipelinePrefixMiddleware("A"),
                new MiddlewarePipelineEmptyMiddleware(),
                new MiddlewarePipelinePrefixMiddleware("B")
            ]
        );

        var removed = pipeline.RemoveMiddleware<MiddlewarePipelineEmptyMiddleware>();
        var output = await pipeline.ExecuteAsync(null, "payload"u8.ToArray(), CancellationToken.None);

        Assert.Multiple(
            () =>
            {
                Assert.That(removed, Is.True);
                Assert.That(output.ToArray(), Is.EqualTo("BApayload"u8.ToArray()));
            }
        );
    }

    [Test]
    public void ContainsMiddleware_ShouldReflectRegisteredMiddlewares()
    {
        var pipeline = new NetMiddlewarePipeline([new MiddlewarePipelinePrefixMiddleware("A")]);

        Assert.Multiple(
            () =>
            {
                Assert.That(pipeline.ContainsMiddleware<MiddlewarePipelinePrefixMiddleware>(), Is.True);
                Assert.That(pipeline.ContainsMiddleware<MiddlewarePipelineEmptyMiddleware>(), Is.False);
            }
        );
    }
}
