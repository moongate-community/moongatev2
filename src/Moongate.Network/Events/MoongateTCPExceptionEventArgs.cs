using Moongate.Network.Client;

namespace Moongate.Network.Events;

/// <summary>
/// Event payload containing an exception raised by server or client network loops.
/// </summary>
public sealed class MoongateTCPExceptionEventArgs(Exception exception, MoongateTCPClient? client = null) : EventArgs
{
    /// <summary>
    /// Exception raised by the networking component.
    /// </summary>
    public Exception Exception { get; } = exception;

    /// <summary>
    /// Client related to the exception, when available.
    /// </summary>
    public MoongateTCPClient? Client { get; } = client;
}
