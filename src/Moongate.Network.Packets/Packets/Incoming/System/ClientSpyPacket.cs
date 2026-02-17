using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.System;

[PacketHandler(0xA4, PacketSizing.Variable)]
public class ClientSpyPacket : BaseGameNetworkPacket
{
    public ClientSpyPacket()
        : base(0xA4, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
