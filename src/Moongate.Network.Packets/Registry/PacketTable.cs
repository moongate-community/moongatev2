namespace Moongate.Network.Packets.Registry;

/// <summary>
/// Registers all known UO protocol packets.
/// Packet sizes from: https://docs.polserver.com/packets/
/// </summary>
public static partial class PacketTable
{
    public static void Register(PacketRegistry registry)
    {
        RegisterGenerated(registry);
    }

    static partial void RegisterGenerated(PacketRegistry registry);
}
