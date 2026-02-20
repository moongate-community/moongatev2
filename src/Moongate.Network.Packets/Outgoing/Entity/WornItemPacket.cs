using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;
using Moongate.UO.Data.Persistence.Entities;
using Moongate.UO.Data.Types;

namespace Moongate.Network.Packets.Outgoing.Entity;

[PacketHandler(0x2E, PacketSizing.Fixed, Length = 15, Description = "Worn Item")]
public class WornItemPacket : BaseGameNetworkPacket
{
    public ItemReference Item { get; set; }

    public ItemLayerType Layer { get; set; }

    public UOMobileEntity? Mobile { get; set; }

    public WornItemPacket()
        : base(0x2E, 15) { }

    public WornItemPacket(UOMobileEntity mobile, ItemReference item, ItemLayerType layer)
        : this()
    {
        Mobile = mobile;
        Item = item;
        Layer = layer;
    }

    public override void Write(ref SpanWriter writer)
    {
        if (Mobile is null)
        {
            throw new InvalidOperationException("Mobile must be set before writing WornItemPacket.");
        }

        writer.Write(OpCode);
        writer.Write(Item.Id.Value);
        writer.Write((ushort)Item.ItemId);
        writer.Write((byte)0);
        writer.Write((byte)Layer);
        writer.Write(Mobile.Id.Value);
        writer.Write((ushort)Item.Hue);
    }

    protected override bool ParsePayload(ref SpanReader reader)
        => reader.Remaining == 14;
}
