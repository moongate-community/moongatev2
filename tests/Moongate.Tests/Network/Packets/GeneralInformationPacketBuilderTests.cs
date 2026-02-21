using System.Buffers.Binary;
using Moongate.Network.Packets.Incoming.System;
using Moongate.Network.Packets.Outgoing.World;
using Moongate.Network.Spans;
using Moongate.UO.Data.Types;

namespace Moongate.Tests.Network.Packets;

public class GeneralInformationPacketBuilderTests
{
    [Test]
    public void CreateSetCursorHueSetMap_ShouldBuildExpectedPacket()
    {
        var packet = GeneralInformationPacketBuilder.CreateSetCursorHueSetMap(2);
        var data = Write(packet);

        Assert.That(data, Is.EqualTo(new byte[] { 0xBF, 0x00, 0x06, 0x00, 0x08, 0x02 }));
    }

    [Test]
    public void CreateMountSpeed_ShouldBuildExpectedPacket()
    {
        var packet = GeneralInformationPacketBuilder.CreateMountSpeed(1);
        var data = Write(packet);

        Assert.Multiple(
            () =>
            {
                Assert.That(data[0], Is.EqualTo(0xBF));
                Assert.That(BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(1, 2)), Is.EqualTo((ushort)6));
                Assert.That(BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(3, 2)), Is.EqualTo((ushort)GeneralInformationSubcommandType.MountSpeed));
                Assert.That(data[5], Is.EqualTo(1));
            }
        );
    }

    [Test]
    public void CreateWrestlingStun_ShouldBuildEmptyPayloadPacket()
    {
        var packet = GeneralInformationPacketBuilder.CreateWrestlingStun();
        var data = Write(packet);

        Assert.That(data, Is.EqualTo(new byte[] { 0xBF, 0x00, 0x05, 0x00, 0x0A }));
    }

    [Test]
    public void Create_ShouldThrow_WhenPayloadInvalid()
    {
        Assert.Throws<ArgumentException>(
            () => GeneralInformationPacketBuilder.Create(GeneralInformationSubcommandType.SetCursorHueSetMap, [])
        );
    }

    private static byte[] Write(GeneralInformationPacket packet)
    {
        var writer = new SpanWriter(64, true);
        packet.Write(ref writer);
        var data = writer.ToArray();
        writer.Dispose();
        return data;
    }
}
