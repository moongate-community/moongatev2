using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;
using Moongate.UO.Data.Persistence.Entities;

namespace Moongate.Network.Packets.Outgoing.Entity;

[PacketHandler(0x3C, PacketSizing.Variable, Description = "Add Multiple Items To Container")]
public class AddMultipleItemsToContainerPacket : BaseGameNetworkPacket
{
    public UOItemEntity? Container { get; set; }

    public AddMultipleItemsToContainerPacket()
        : base(0x3C) { }

    public AddMultipleItemsToContainerPacket(UOItemEntity container)
        : this()
        => Container = container;

    public override void Write(ref SpanWriter writer)
    {
        if (Container is null)
        {
            throw new InvalidOperationException("Container must be set before writing AddMultipleItemsToContainerPacket.");
        }

        var items = Container.Items;
        var totalItems = items.Count;
        var length = 5 + totalItems * 20;

        writer.Write(OpCode);
        writer.Write((ushort)length);
        writer.Write((ushort)totalItems);

        for (var i = 0; i < totalItems; i++)
        {
            var (item, position) = items[i];

            writer.Write(item.Id.Value);
            writer.Write((ushort)item.ItemId);
            writer.Write((byte)0);
            writer.Write((short)1);
            writer.Write((short)position.X);
            writer.Write((short)position.Y);
            writer.Write((byte)i);
            writer.Write(Container.Id.Value);
            writer.Write((short)item.Hue);
        }
    }

    protected override bool ParsePayload(ref SpanReader reader)
        => reader.Remaining >= 4;
}
