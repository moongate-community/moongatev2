using System.Text;
using Moongate.Network.Client;
using Moongate.Network.Interfaces;
using Moongate.Network.Pipeline;

namespace Moongate.Tests.Network;

public class MiddlewarePipelineTests
{
    private sealed class PrefixMiddleware(string prefix) : INetMiddleware
    {
        public ValueTask<ReadOnlyMemory<byte>> ProcessAsync(
            MoongateTCPClient? client,
            ReadOnlyMemory<byte> data,
            CancellationToken cancellationToken = default
        )
        {
            var prefixBytes = Encoding.UTF8.GetBytes(prefix);
            var combined = new byte[prefixBytes.Length + data.Length];

            prefixBytes.CopyTo(combined, 0);
            data.CopyTo(combined.AsMemory(prefixBytes.Length));

            return ValueTask.FromResult<ReadOnlyMemory<byte>>(combined);
        }
    }

    private sealed class EmptyMiddleware : INetMiddleware
    {
        public ValueTask<ReadOnlyMemory<byte>> ProcessAsync(
            MoongateTCPClient? client,
            ReadOnlyMemory<byte> data,
            CancellationToken cancellationToken = default
        )
            => ValueTask.FromResult(ReadOnlyMemory<byte>.Empty);
    }

    private sealed class SendPrefixMiddleware(string prefix) : INetMiddleware
    {
        public ValueTask<ReadOnlyMemory<byte>> ProcessAsync(
            MoongateTCPClient? client,
            ReadOnlyMemory<byte> data,
            CancellationToken cancellationToken = default
        )
            => ValueTask.FromResult(data);

        public ValueTask<ReadOnlyMemory<byte>> ProcessSendAsync(
            MoongateTCPClient? client,
            ReadOnlyMemory<byte> data,
            CancellationToken cancellationToken = default
        )
        {
            var prefixBytes = Encoding.UTF8.GetBytes(prefix);
            var combined = new byte[prefixBytes.Length + data.Length];

            prefixBytes.CopyTo(combined, 0);
            data.CopyTo(combined.AsMemory(prefixBytes.Length));

            return ValueTask.FromResult<ReadOnlyMemory<byte>>(combined);
        }
    }

    [Test]
    public async Task ExecuteAsync_ShouldProcessMiddlewaresInOrder()
    {
        var pipeline = new NetMiddlewarePipeline(
            [
                new PrefixMiddleware("A"),
                new PrefixMiddleware("B")
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
                new EmptyMiddleware(),
                new PrefixMiddleware("X")
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
                new SendPrefixMiddleware("A"),
                new SendPrefixMiddleware("B")
            ]
        );

        var input = "payload"u8.ToArray();
        var output = await pipeline.ExecuteSendAsync(null, input, CancellationToken.None);

        Assert.That(output.ToArray(), Is.EqualTo("BApayload"u8.ToArray()));
    }
}
