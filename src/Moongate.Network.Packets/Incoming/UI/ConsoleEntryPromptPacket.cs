using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.UI;

[PacketHandler(0x9A, PacketSizing.Variable, Description = "Console Entry Prompt")]
public class ConsoleEntryPromptPacket : BaseGameNetworkPacket
{
    public ConsoleEntryPromptPacket()
        : base(0x9A) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
