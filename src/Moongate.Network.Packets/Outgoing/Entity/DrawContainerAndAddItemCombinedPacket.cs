using Moongate.Network.Packets.Base;
using Moongate.Network.Spans;
using Moongate.UO.Data.Persistence.Entities;

namespace Moongate.Network.Packets.Outgoing.Entity;

/// <summary>
/// Composite outbound packet that writes container open and contained items in a single payload.
/// This helper is intentionally not registered because it wraps multiple protocol packets.
/// </summary>
public class DrawContainerAndAddItemCombinedPacket : BaseGameNetworkPacket
{
    public UOItemEntity? Container { get; set; }

    public DrawContainerAndAddItemCombinedPacket()
        : base(0x01) { }

    public DrawContainerAndAddItemCombinedPacket(UOItemEntity container)
        : this()
        => Container = container;

    public override void Write(ref SpanWriter writer)
    {
        if (Container is null)
        {
            throw new InvalidOperationException("Container must be set before writing DrawContainerAndAddItemCombinedPacket.");
        }

        var drawContainer = new DrawContainerPacket(Container);
        drawContainer.Write(ref writer);

        var addItems = new AddMultipleItemsToContainerPacket(Container);
        addItems.Write(ref writer);
    }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
