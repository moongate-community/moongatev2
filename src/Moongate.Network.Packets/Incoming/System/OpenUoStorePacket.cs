using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.System;

[PacketHandler(0xFA, PacketSizing.Fixed, Length = 1, Description = "Open UO Store")]
public class OpenUoStorePacket : BaseGameNetworkPacket
{
    public OpenUoStorePacket()
        : base(0xFA, 1) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
