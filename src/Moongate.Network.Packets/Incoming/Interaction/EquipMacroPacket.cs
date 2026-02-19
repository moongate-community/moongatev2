using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Interaction;

[PacketHandler(0xEC, PacketSizing.Variable, Description = "Equip Macro (KR)")]
public class EquipMacroPacket : BaseGameNetworkPacket
{
    public EquipMacroPacket()
        : base(0xEC) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
