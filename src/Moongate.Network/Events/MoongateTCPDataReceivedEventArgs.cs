using Moongate.Network.Client;

namespace Moongate.Network.Events;

/// <summary>
/// Event payload containing data received from a network client.
/// </summary>
public sealed class MoongateTCPDataReceivedEventArgs(MoongateTCPClient client, ReadOnlyMemory<byte> data) : EventArgs
{
    /// <summary>
    /// Source client for the data payload.
    /// </summary>
    public MoongateTCPClient Client { get; } = client;

    /// <summary>
    /// Received data payload.
    /// </summary>
    public ReadOnlyMemory<byte> Data { get; } = data;
}
