using Moongate.Network.Compression;
using Moongate.UO.Data.Middlewares;

namespace Moongate.Tests.UO.Data.Middlewares;

public class CompressionMiddlewareTests
{
    [Test]
    public async Task ProcessAsync_ShouldPassThroughInboundData()
    {
        var middleware = new CompressionMiddleware();
        var payload = new byte[] { 0x80, 0x00, 0x3E, 0x41, 0x42, 0x43 };

        var output = await middleware.ProcessAsync(null, payload);

        Assert.That(output.ToArray(), Is.EqualTo(payload));
    }

    [Test]
    public async Task ProcessSendAsync_ShouldCompressOutboundData()
    {
        var middleware = new CompressionMiddleware();
        var payload = Enumerable.Range(0, 64).Select(static i => (byte)i).ToArray();

        var compressed = await middleware.ProcessSendAsync(null, payload);
        Span<byte> decompressedBuffer = stackalloc byte[NetworkCompression.BufferSize];
        var decompressedLength = NetworkCompression.Decompress(compressed.Span, decompressedBuffer);
        var roundtrip = decompressedBuffer[..decompressedLength].ToArray();

        Assert.Multiple(
            () =>
            {
                Assert.That(compressed.IsEmpty, Is.False);
                Assert.That(decompressedLength, Is.EqualTo(payload.Length));
                Assert.That(roundtrip, Is.EqualTo(payload));
            }
        );
    }
}
