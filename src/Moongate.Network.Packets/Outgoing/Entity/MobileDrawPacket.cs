using Moongate.UO.Data.Persistence.Entities;

namespace Moongate.Network.Packets.Outgoing.Entity;

/// <summary>
/// Compatibility packet for legacy code paths that reference MobileDrawPacket (0x78).
/// </summary>
public class MobileDrawPacket : MobileIncomingPacket
{
    public MobileDrawPacket()
    {
        StygianAbyss = false;
        NewMobileIncoming = false;
    }

    public MobileDrawPacket(
        UOMobileEntity beholder,
        UOMobileEntity beheld,
        bool stygianAbyss = false,
        bool newMobileIncoming = false
    )
        : this()
    {
        Beholder = beholder;
        Beheld = beheld;
        StygianAbyss = stygianAbyss;
        NewMobileIncoming = newMobileIncoming;
    }
}
