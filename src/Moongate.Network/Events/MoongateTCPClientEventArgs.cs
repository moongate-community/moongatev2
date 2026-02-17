using Moongate.Network.Client;

namespace Moongate.Network.Events;

/// <summary>
/// Event payload containing a network client instance.
/// </summary>
public sealed class MoongateTCPClientEventArgs(MoongateTCPClient client) : EventArgs
{
    /// <summary>
    /// Connected or disconnected client.
    /// </summary>
    public MoongateTCPClient Client { get; } = client;
}
