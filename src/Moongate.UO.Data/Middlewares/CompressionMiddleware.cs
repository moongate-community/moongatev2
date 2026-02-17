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

        var input = data;
        NetworkCompression.ProcessSend(ref input, out var output);

        return ValueTask.FromResult(output);
    }
}
