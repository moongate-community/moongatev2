using Moongate.Network.Client;
using Moongate.Network.Compression;
using Moongate.Network.Interfaces;

namespace Moongate.UO.Data.Middlewares;

/// <summary>
/// Middleware that transparently decompresses inbound payloads and compresses outbound payloads.
/// </summary>
public sealed class CompressionMiddleware : INetMiddleware
{
    /// <inheritdoc />
    public ValueTask<ReadOnlyMemory<byte>> ProcessAsync(
        MoongateTCPClient? client,
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken = default
    )
    {
        _ = client;
        cancellationToken.ThrowIfCancellationRequested();

        var input = data;
        var result = NetworkCompression.ProcessReceive(ref input, out var output);

        if (result.halt)
        {
            return ValueTask.FromResult(ReadOnlyMemory<byte>.Empty);
        }

        return ValueTask.FromResult(output);
    }

    /// <inheritdoc />
    public ValueTask<ReadOnlyMemory<byte>> ProcessSendAsync(
        MoongateTCPClient? client,
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken = default
    )
    {
        _ = client;
        cancellationToken.ThrowIfCancellationRequested();

        if (data.IsEmpty)
        {
            return ValueTask.FromResult(ReadOnlyMemory<byte>.Empty);
        }

        var maxSize = NetworkCompression.CalculateMaxCompressedSize(data.Length);

        if (maxSize <= 0)
        {
            return ValueTask.FromResult(ReadOnlyMemory<byte>.Empty);
        }

        var buffer = new byte[maxSize];
        var compressedLength = NetworkCompression.Compress(data.Span, buffer.AsSpan());

        if (compressedLength <= 0)
        {
            return ValueTask.FromResult(ReadOnlyMemory<byte>.Empty);
        }

        return ValueTask.FromResult<ReadOnlyMemory<byte>>(buffer.AsMemory(0, compressedLength));
    }
}
