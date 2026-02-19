using System.Text;
using Moongate.Network.Client;
using Moongate.Network.Interfaces;

namespace Moongate.Tests.Network.Support;

public sealed class MiddlewarePipelineSendPrefixMiddleware : INetMiddleware
{
    private readonly string _prefix;

    public MiddlewarePipelineSendPrefixMiddleware(string prefix)
        => _prefix = prefix;

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
        var prefixBytes = Encoding.UTF8.GetBytes(_prefix);
        var combined = new byte[prefixBytes.Length + data.Length];

        prefixBytes.CopyTo(combined, 0);
        data.CopyTo(combined.AsMemory(prefixBytes.Length));

        return ValueTask.FromResult<ReadOnlyMemory<byte>>(combined);
    }
}
