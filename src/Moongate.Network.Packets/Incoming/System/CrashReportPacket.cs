using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.System;

[PacketHandler(0xF4, PacketSizing.Variable, Description = "CrashReport")]
public class CrashReportPacket : BaseGameNetworkPacket
{
    public CrashReportPacket()
        : base(0xF4) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
