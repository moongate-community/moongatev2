using Moongate.Network.Client;
using Moongate.Network.Interfaces;

namespace Moongate.Network.Pipeline;

/// <summary>
/// Executes network middleware components in registration order.
/// </summary>
public sealed class NetMiddlewarePipeline(IEnumerable<INetMiddleware>? middlewares = null)
{
    private readonly INetMiddleware[] _middlewares = [.. middlewares ?? []];

    /// <summary>
    /// Processes the payload through all registered middleware components.
    /// </summary>
    /// <param name="client">Client associated with the payload, if available.</param>
    /// <param name="data">Incoming payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The processed payload, or empty when dropped by middleware.</returns>
    public async ValueTask<ReadOnlyMemory<byte>> ExecuteAsync(
        MoongateTCPClient? client,
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken
    )
    {
        var current = data;

        for (var i = 0; i < _middlewares.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            current = await _middlewares[i].ProcessAsync(client, current, cancellationToken);

            if (current.IsEmpty)
            {
                return ReadOnlyMemory<byte>.Empty;
            }
        }

        return current;
    }

    /// <summary>
    /// Processes the outgoing payload through all registered middleware components.
    /// </summary>
    /// <param name="client">Client associated with the payload, if available.</param>
    /// <param name="data">Outgoing payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The processed payload, or empty when dropped by middleware.</returns>
    public async ValueTask<ReadOnlyMemory<byte>> ExecuteSendAsync(
        MoongateTCPClient? client,
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken
    )
    {
        var current = data;

        for (var i = 0; i < _middlewares.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            current = await _middlewares[i].ProcessSendAsync(client, current, cancellationToken);

            if (current.IsEmpty)
            {
                return ReadOnlyMemory<byte>.Empty;
            }
        }

        return current;
    }
}
