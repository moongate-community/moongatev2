using System.Buffers.Binary;
using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;
using Moongate.UO.Data.Types;

namespace Moongate.Network.Packets.Outgoing.Entity;

[PacketHandler(0x78, PacketSizing.Variable, Description = "Draw Object")]
public class MobileIncomingPacket : BaseGameNetworkPacket
{
    private const uint FacialHairVirtualSerialBase = 0x7F800000;
    private const uint HairVirtualSerialBase = 0x7F000000;

    public UOMobileEntity? Beholder { get; set; }

    public UOMobileEntity? Beheld { get; set; }

    public bool StygianAbyss { get; set; } = true;

    public bool NewMobileIncoming { get; set; } = true;

    public MobileIncomingPacket()
        : base(0x78) { }

    public MobileIncomingPacket(
        UOMobileEntity beholder,
        UOMobileEntity beheld,
        bool stygianAbyss = true,
        bool newMobileIncoming = true
    ) : this()
    {
        Beholder = beholder;
        Beheld = beheld;
        StygianAbyss = stygianAbyss;
        NewMobileIncoming = newMobileIncoming;
    }

    public override void Write(ref SpanWriter writer)
    {
        if (Beheld is null)
        {
            throw new InvalidOperationException("Beheld mobile must be set before writing MobileIncomingPacket.");
        }

        var start = writer.Position;
        Span<bool> layers = stackalloc bool[256];
        layers.Clear();
        var itemIdMask = NewMobileIncoming ? 0xFFFF : 0x7FFF;

        writer.Write(OpCode);
        writer.Write((ushort)0);
        writer.Write(Beheld.Id.Value);
        writer.Write((short)Beheld.Body);
        writer.Write((short)Beheld.Location.X);
        writer.Write((short)Beheld.Location.Y);
        writer.Write((sbyte)Beheld.Location.Z);
        writer.Write((byte)Beheld.Direction);
        writer.Write(Beheld.SkinHue);
        writer.Write(Beheld.GetPacketFlags(StygianAbyss));
        writer.Write((byte)Beheld.Notoriety);

        foreach (var (layer, item) in Beheld.EquippedItemReferences)
        {
            var layerByte = (byte)layer;

            if (layerByte >= 0xFF || layers[layerByte])
            {
                continue;
            }

            if (Beheld != Beholder && !IsVisibleLayer(layer))
            {
                continue;
            }

            layers[layerByte] = true;
            var itemId = item.ItemId & itemIdMask;
            var writeHue = NewMobileIncoming || item.Hue != 0;

            if (!NewMobileIncoming && writeHue)
            {
                itemId |= 0x8000;
            }

            writer.Write(item.Id.Value);
            writer.Write((ushort)itemId);
            writer.Write(layerByte);

            if (writeHue)
            {
                writer.Write((ushort)item.Hue);
            }
        }

        if (Beheld.HairStyle > 0 && !layers[(byte)ItemLayerType.Hair])
        {
            layers[(byte)ItemLayerType.Hair] = true;
            var hairItemId = Beheld.HairStyle & itemIdMask;
            var writeHue = NewMobileIncoming || Beheld.HairHue != 0;

            if (!NewMobileIncoming && writeHue)
            {
                hairItemId |= 0x8000;
            }

            writer.Write(GetVirtualHairSerial(Beheld.Id, isFacialHair: false));
            writer.Write((ushort)hairItemId);
            writer.Write((byte)ItemLayerType.Hair);

            if (writeHue)
            {
                writer.Write((ushort)Beheld.HairHue);
            }
        }

        if (Beheld.FacialHairStyle > 0 && !layers[(byte)ItemLayerType.FacialHair])
        {
            layers[(byte)ItemLayerType.FacialHair] = true;
            var facialHairItemId = Beheld.FacialHairStyle & itemIdMask;
            var writeHue = NewMobileIncoming || Beheld.FacialHairHue != 0;

            if (!NewMobileIncoming && writeHue)
            {
                facialHairItemId |= 0x8000;
            }

            writer.Write(GetVirtualHairSerial(Beheld.Id, isFacialHair: true));
            writer.Write((ushort)facialHairItemId);
            writer.Write((byte)ItemLayerType.FacialHair);

            if (writeHue)
            {
                writer.Write((ushort)Beheld.FacialHairHue);
            }
        }

        writer.Write(0);
        var packetLength = (ushort)(writer.Position - start);
        BinaryPrimitives.WriteUInt16BigEndian(writer.RawBuffer[(start + 1)..], packetLength);
    }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;

    private static bool IsVisibleLayer(ItemLayerType layer)
        => layer != ItemLayerType.Backpack &&
           layer != ItemLayerType.Bank &&
           layer != ItemLayerType.ShopBuy &&
           layer != ItemLayerType.ShopSell &&
           layer != ItemLayerType.ShopResale;

    private static uint GetVirtualHairSerial(Serial mobileId, bool isFacialHair)
    {
        // Keep virtual serials in non-item range to avoid collisions with real equipped item serials.
        var baseValue = isFacialHair ? FacialHairVirtualSerialBase : HairVirtualSerialBase;
        return baseValue | (mobileId.Value & 0x007FFFFF);
    }
}
