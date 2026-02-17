using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.System;

[PacketHandler(0xD0, PacketSizing.Variable)]
public class ConfigurationFilePacket : BaseGameNetworkPacket
{
    public ConfigurationFilePacket()
        : base(0xD0, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
