using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;
using Moongate.UO.Data.Types;

namespace Moongate.Network.Packets.Incoming.Player;

[PacketHandler(0x34, PacketSizing.Fixed, Length = 10, Description = "Get Player Status")]
public class GetPlayerStatusPacket : BaseGameNetworkPacket
{
    public uint MobileSerial { get; set; }

    public GetPlayerStatusType StatusType { get; set; }

    public uint UnknownPattern { get; set; }

    public GetPlayerStatusPacket()
        : base(0x34, 10) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 9)
        {
            return false;
        }

        UnknownPattern = reader.ReadUInt32();
        StatusType = (GetPlayerStatusType)reader.ReadByte();
        MobileSerial = reader.ReadUInt32();

        return reader.Remaining == 0;
    }
}
