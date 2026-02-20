using System.Buffers.Binary;
using Moongate.Network.Packets.Incoming.Login;
using Moongate.Network.Packets.Incoming.System;
using Moongate.Network.Packets.Outgoing.Entity;
using Moongate.Network.Packets.Outgoing.Login;
using Moongate.Network.Packets.Outgoing.World;
using Moongate.Network.Spans;
using Moongate.UO.Data.Geometry;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;
using Moongate.UO.Data.Types;
using UOMap = Moongate.UO.Data.Maps.Map;

namespace Moongate.Tests.Network.Packets;

public class AfterLoginOutgoingPacketsTests
{
    [Test]
    public void LoginConfirmPacket_Write_ShouldSerializeFixedPayload()
    {
        var mobile = CreateMobile();
        var packet = new LoginConfirmPacket(mobile);

        var data = Write(packet);

        Assert.Multiple(
            () =>
            {
                Assert.That(data.Length, Is.EqualTo(37));
                Assert.That(data[0], Is.EqualTo(0x1B));
                Assert.That(BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(1, 4)), Is.EqualTo(mobile.Id.Value));
                Assert.That(BinaryPrimitives.ReadInt16BigEndian(data.AsSpan(9, 2)), Is.EqualTo((short)mobile.Body));
                Assert.That(BinaryPrimitives.ReadInt16BigEndian(data.AsSpan(11, 2)), Is.EqualTo((short)mobile.Location.X));
                Assert.That(BinaryPrimitives.ReadInt16BigEndian(data.AsSpan(13, 2)), Is.EqualTo((short)mobile.Location.Y));
            }
        );
    }

    [Test]
    public void DrawPlayerPacket_Write_ShouldSerializeExpectedFields()
    {
        var mobile = CreateMobile();
        var packet = new DrawPlayerPacket(mobile);

        var data = Write(packet);

        Assert.Multiple(
            () =>
            {
                Assert.That(data.Length, Is.EqualTo(19));
                Assert.That(data[0], Is.EqualTo(0x20));
                Assert.That(BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(1, 4)), Is.EqualTo(mobile.Id.Value));
                Assert.That(BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(8, 2)), Is.EqualTo((ushort)mobile.SkinHue));
            }
        );
    }

    [Test]
    public void MobileIncomingPacket_Write_ShouldSerializeHeaderAndTerminator()
    {
        var beholder = CreateMobile();
        var beheld = CreateMobile();
        var packet = new MobileIncomingPacket(beholder, beheld);

        var data = Write(packet);

        Assert.Multiple(
            () =>
            {
                Assert.That(data[0], Is.EqualTo(0x78));
                Assert.That(BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(1, 2)), Is.EqualTo((ushort)data.Length));
                Assert.That(BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(data.Length - 4, 4)), Is.EqualTo(0u));
            }
        );
    }

    [Test]
    public void MobileDrawPacket_Write_ShouldSerializeHeaderAndTerminator()
    {
        var beholder = CreateMobile();
        var beheld = CreateMobile();
        var packet = new MobileDrawPacket(beholder, beheld, stygianAbyss: true, newMobileIncoming: true);

        var data = Write(packet);

        Assert.Multiple(
            () =>
            {
                Assert.That(data[0], Is.EqualTo(0x78));
                Assert.That(BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(1, 2)), Is.EqualTo((ushort)data.Length));
                Assert.That(BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(data.Length - 4, 4)), Is.EqualTo(0u));
            }
        );
    }

    [Test]
    public void WornItemPacket_Write_ShouldSerializeFixedPayload()
    {
        var mobile = CreateMobile();
        var item = new ItemReference((Serial)0x40000010, 0x1515, 0x0444);
        var packet = new WornItemPacket(mobile, item, ItemLayerType.Shirt);

        var data = Write(packet);

        Assert.Multiple(
            () =>
            {
                Assert.That(data.Length, Is.EqualTo(15));
                Assert.That(data[0], Is.EqualTo(0x2E));
                Assert.That(BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(1, 4)), Is.EqualTo(item.Id.Value));
                Assert.That(data[8], Is.EqualTo((byte)ItemLayerType.Shirt));
                Assert.That(BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(9, 4)), Is.EqualTo(mobile.Id.Value));
            }
        );
    }

    [Test]
    public void WarModePacket_Write_ShouldSerializeExpectedPayload()
    {
        var mobile = CreateMobile();
        mobile.IsWarMode = true;
        var packet = new WarModePacket(mobile);

        var data = Write(packet);

        Assert.Multiple(
            () =>
            {
                Assert.That(data.Length, Is.EqualTo(5));
                Assert.That(data[0], Is.EqualTo(0x72));
                Assert.That(data[1], Is.EqualTo(1));
                Assert.That(data[3], Is.EqualTo(0x32));
            }
        );
    }

    [Test]
    public void OverallLightLevelPacket_Write_ShouldSerializeExpectedPayload()
    {
        var packet = new OverallLightLevelPacket(LightLevelType.Day);

        var data = Write(packet);

        Assert.That(data, Is.EqualTo(new byte[] { 0x4F, (byte)LightLevelType.Day }));
    }

    [Test]
    public void PersonalLightLevelPacket_Write_ShouldSerializeExpectedPayload()
    {
        var mobile = CreateMobile();
        var packet = new PersonalLightLevelPacket(LightLevelType.OsiNight, mobile);

        var data = Write(packet);

        Assert.Multiple(
            () =>
            {
                Assert.That(data.Length, Is.EqualTo(6));
                Assert.That(data[0], Is.EqualTo(0x4E));
                Assert.That(BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(1, 4)), Is.EqualTo(mobile.Id.Value));
                Assert.That(data[5], Is.EqualTo((byte)LightLevelType.OsiNight));
            }
        );
    }

    [Test]
    public void SeasonPacket_Write_ShouldSerializeExpectedPayload()
    {
        var packet = new SeasonPacket(SeasonType.Winter, playSound: true);

        var data = Write(packet);

        Assert.That(data, Is.EqualTo(new byte[] { 0xBC, (byte)SeasonType.Winter, 0x01 }));
    }

    [Test]
    public void LoginCompletePacket_Write_ShouldSerializeOpcodeOnly()
    {
        var packet = new LoginCompletePacket();

        var data = Write(packet);

        Assert.That(data, Is.EqualTo(new byte[] { 0x55 }));
    }

    [Test]
    public void SetTimePacket_Write_ShouldSerializeTimeComponents()
    {
        var time = new DateTime(2026, 2, 20, 14, 30, 45, DateTimeKind.Utc);
        var packet = new SetTimePacket(time);

        var data = Write(packet);

        Assert.That(data, Is.EqualTo(new byte[] { 0x5B, 14, 30, 45 }));
    }

    [Test]
    public void SetMusicPacket_Write_ShouldSerializeMusicId()
    {
        var packet = new SetMusicPacket(MusicName.Britain1);

        var data = Write(packet);

        Assert.Multiple(
            () =>
            {
                Assert.That(data.Length, Is.EqualTo(3));
                Assert.That(data[0], Is.EqualTo(0x6D));
                Assert.That(BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(1, 2)), Is.EqualTo((ushort)MusicName.Britain1));
            }
        );
    }

    [Test]
    public void PaperdollPacket_Write_ShouldSerializeHeaderLengthAndFlags()
    {
        var mobile = CreateMobile();
        var packet = new PaperdollPacket(mobile, "the brave");

        var data = Write(packet);

        Assert.Multiple(
            () =>
            {
                Assert.That(data[0], Is.EqualTo(0x88));
                Assert.That(BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(1, 2)), Is.EqualTo((ushort)data.Length));
                Assert.That(BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(3, 4)), Is.EqualTo(mobile.Id.Value));
            }
        );
    }

    [Test]
    public void ClientVersionPacket_Write_ShouldSerializeMinimalPayload()
    {
        var packet = new ClientVersionPacket();

        var data = Write(packet);

        Assert.That(data, Is.EqualTo(new byte[] { 0xBD, 0x00, 0x03 }));
    }

    [Test]
    public void GeneralInformationPacket_Write_ShouldSerializeSubcommandPayload()
    {
        var packet = GeneralInformationPacket.CreateSetCursorHueSetMap(2);

        var data = Write(packet);

        Assert.Multiple(
            () =>
            {
                Assert.That(data[0], Is.EqualTo(0xBF));
                Assert.That(BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(1, 2)), Is.EqualTo((ushort)data.Length));
                Assert.That(BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(3, 2)), Is.EqualTo((ushort)GeneralInformationSubcommandType.SetCursorHueSetMap));
                Assert.That(data[5], Is.EqualTo(2));
            }
        );
    }

    [Test]
    public void GeneralInformationFactory_CreateSetCursorHueSetMap_WithByte_ShouldSerializeMapId()
    {
        var packet = GeneralInformationFactory.CreateSetCursorHueSetMap(4);

        var data = Write(packet);

        Assert.That(data, Is.EqualTo(new byte[] { 0xBF, 0x00, 0x06, 0x00, 0x08, 0x04 }));
    }

    [Test]
    public void GeneralInformationFactory_CreateSetCursorHueSetMap_WithNullMap_ShouldSerializeDefaultMapIdZero()
    {
        var packet = GeneralInformationFactory.CreateSetCursorHueSetMap((UOMap?)null);

        var data = Write(packet);

        Assert.That(data, Is.EqualTo(new byte[] { 0xBF, 0x00, 0x06, 0x00, 0x08, 0x00 }));
    }

    [Test]
    public void DrawContainerAndAddItemCombinedPacket_Write_ShouldSerializeTwoPackets()
    {
        var container = new UOItemEntity
        {
            Id = (Serial)0x40000040,
            GumpId = 0x003C
        };
        var item = new UOItemEntity
        {
            Id = (Serial)0x40000041,
            ItemId = 0x0EED,
            Hue = 0x0444
        };
        container.AddItem(item, new Point2D(12, 34));

        var packet = new DrawContainerAndAddItemCombinedPacket(container);

        var data = Write(packet);

        Assert.Multiple(
            () =>
            {
                Assert.That(data[0], Is.EqualTo(0x24));
                Assert.That(data[9], Is.EqualTo(0x3C));
                Assert.That(BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(10, 2)), Is.EqualTo((ushort)(5 + 20)));
                Assert.That(BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(12, 2)), Is.EqualTo((ushort)1));
            }
        );
    }

    private static UOMobileEntity CreateMobile()
    {
        var mobile = new UOMobileEntity
        {
            Id = (Serial)0x0000001A,
            Name = "TestPlayer",
            Location = new(1000, 1200, 7),
            Direction = DirectionType.East,
            SkinHue = 0x0835,
            Gender = GenderType.Male,
            Notoriety = Notoriety.Innocent,
            RaceIndex = 1
        };

        mobile.SetBody((Moongate.UO.Data.Bodies.Body)0x0190);

        return mobile;
    }

    private static byte[] Write(Moongate.Network.Packets.Interfaces.IGameNetworkPacket packet)
    {
        var writer = new SpanWriter(256, true);
        packet.Write(ref writer);
        var data = writer.ToArray();
        writer.Dispose();

        return data;
    }
}
