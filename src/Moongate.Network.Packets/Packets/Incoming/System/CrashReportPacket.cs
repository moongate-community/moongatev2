using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.System;

[PacketHandler(0xF4, PacketSizing.Variable)]
public class CrashReportPacket : BaseGameNetworkPacket
{
    public CrashReportPacket()
        : base(0xF4, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
