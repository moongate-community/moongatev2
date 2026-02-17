using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.System;

[PacketHandler(0xD0, PacketSizing.Variable, Description = "Configuration File")]
public class ConfigurationFilePacket : BaseGameNetworkPacket
{
    public ConfigurationFilePacket()
        : base(0xD0) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
