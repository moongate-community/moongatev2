namespace Moongate.Server.Interfaces.Services.Packets;

/// <summary>
/// Background service that drains the outgoing packet queue and sends packets
/// to connected clients on a dedicated thread.
/// </summary>
public interface IPacketSenderService
{
    /// <summary>Starts the sender thread.</summary>
    Task StartAsync();

    /// <summary>Stops the sender thread.</summary>
    Task StopAsync();
}
