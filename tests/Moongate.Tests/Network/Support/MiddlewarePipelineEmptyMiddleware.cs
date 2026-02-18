using Moongate.Network.Client;
using Moongate.Network.Interfaces;

namespace Moongate.Tests.Network.Support;

public sealed class MiddlewarePipelineEmptyMiddleware : INetMiddleware
{
    public ValueTask<ReadOnlyMemory<byte>> ProcessAsync(
        MoongateTCPClient? client,
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken = default
    )
        => ValueTask.FromResult(ReadOnlyMemory<byte>.Empty);
}
