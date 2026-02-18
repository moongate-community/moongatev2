using System.Text;
using Moongate.Network.Client;
using Moongate.Network.Interfaces;

namespace Moongate.Tests.Network.Support;

public sealed class MoongateTcpServerUppercaseMiddleware : INetMiddleware
{
    public ValueTask<ReadOnlyMemory<byte>> ProcessAsync(
        MoongateTCPClient? client,
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken = default
    )
    {
        var value = Encoding.UTF8.GetString(data.Span).ToUpperInvariant();
        return ValueTask.FromResult<ReadOnlyMemory<byte>>(Encoding.UTF8.GetBytes(value));
    }
}
