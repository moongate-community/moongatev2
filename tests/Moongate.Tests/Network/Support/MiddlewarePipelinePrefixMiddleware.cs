using System.Text;
using Moongate.Network.Client;
using Moongate.Network.Interfaces;

namespace Moongate.Tests.Network.Support;

public sealed class MiddlewarePipelinePrefixMiddleware : INetMiddleware
{
    private readonly string _prefix;

    public MiddlewarePipelinePrefixMiddleware(string prefix)
        => _prefix = prefix;

    public ValueTask<ReadOnlyMemory<byte>> ProcessAsync(
        MoongateTCPClient? client,
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken = default
    )
    {
        var prefixBytes = Encoding.UTF8.GetBytes(_prefix);
        var combined = new byte[prefixBytes.Length + data.Length];

        prefixBytes.CopyTo(combined, 0);
        data.CopyTo(combined.AsMemory(prefixBytes.Length));

        return ValueTask.FromResult<ReadOnlyMemory<byte>>(combined);
    }
}
