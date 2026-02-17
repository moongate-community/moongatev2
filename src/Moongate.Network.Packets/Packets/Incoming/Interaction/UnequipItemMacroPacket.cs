using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Interaction;

[PacketHandler(0xED, PacketSizing.Variable, Description = "Unequip Item Macro (KR)")]
public class UnequipItemMacroPacket : BaseGameNetworkPacket
{
    public UnequipItemMacroPacket()
        : base(0xED) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
