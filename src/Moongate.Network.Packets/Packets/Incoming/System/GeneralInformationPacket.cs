using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.System;

[PacketHandler(0xBF, PacketSizing.Variable, Description = "General Information Packet")]
public class GeneralInformationPacket : BaseGameNetworkPacket
{
    public GeneralInformationPacket()
        : base(0xBF, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
