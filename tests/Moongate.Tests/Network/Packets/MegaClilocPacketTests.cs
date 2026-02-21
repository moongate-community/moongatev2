using System.Buffers.Binary;
using Moongate.Network.Packets.Incoming.System;
using Moongate.Network.Packets.Outgoing.System;
using Moongate.Network.Spans;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.MegaCliloc;

namespace Moongate.Tests.Network.Packets;

public class MegaClilocPacketTests
{
    [Test]
    public void MegaClilocPacket_TryParse_ClientRequest_ShouldReadRequestedSerials()
    {
        var raw = new byte[] { 0xD6, 0x00, 0x0B, 0x40, 0x00, 0x00, 0x01, 0x40, 0x00, 0x00, 0x02 };
        var packet = new MegaClilocPacket();

        var ok = packet.TryParse(raw);

        Assert.Multiple(
            () =>
            {
                Assert.That(ok, Is.True);
                Assert.That(packet.IsClientRequest, Is.True);
                Assert.That(packet.RequestedSerials.Count, Is.EqualTo(2));
                Assert.That(packet.RequestedSerials[0], Is.EqualTo((Serial)0x40000001));
                Assert.That(packet.RequestedSerials[1], Is.EqualTo((Serial)0x40000002));
            }
        );
    }

    [Test]
    public void MegaClilocPacket_TryParse_ServerResponse_ShouldReadProperties()
    {
        var raw = new byte[]
        {
            0xD6, 0x00, 0x19,
            0x00, 0x01,
            0x40, 0x00, 0x00, 0x02,
            0x00, 0x00,
            0x40, 0x00, 0x00, 0x02,
            0x00, 0x0F, 0x42, 0x40,
            0x00, 0x00,
            0x00, 0x00, 0x00, 0x00
        };
        var packet = new MegaClilocPacket();

        var ok = packet.TryParse(raw);

        Assert.Multiple(
            () =>
            {
                Assert.That(ok, Is.True);
                Assert.That(packet.IsClientRequest, Is.False);
                Assert.That(packet.Subcommand, Is.EqualTo(0x0001));
                Assert.That(packet.Serial, Is.EqualTo((Serial)0x40000002));
                Assert.That(packet.Properties.Count, Is.EqualTo(1));
                Assert.That(packet.Properties[0].ClilocId, Is.EqualTo(1_000_000u));
                Assert.That(packet.Properties[0].Text, Is.Null);
            }
        );
    }

    [Test]
    public void ObjectPropertyList_Write_ShouldSerializeHeaderAndTerminator()
    {
        using var packet = new ObjectPropertyList((Serial)0x40000010);
        packet.Add(1_000_000u, "Test");

        var bytes = Write(packet);

        Assert.Multiple(
            () =>
            {
                Assert.That(bytes[0], Is.EqualTo(0xD6));
                Assert.That(BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(1, 2)), Is.EqualTo((ushort)bytes.Length));
                Assert.That(BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(3, 2)), Is.EqualTo((ushort)0x0001));
                Assert.That(BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(bytes.Length - 4, 4)), Is.EqualTo(0u));
            }
        );
    }

    [Test]
    public void ObjectPropertyList_Write_ShouldBeParsableByMegaClilocPacket()
    {
        using var outgoing = new ObjectPropertyList((Serial)0x40000011);
        outgoing.Add(CommonClilocIds.ObjectName, "Moongate");
        
        var bytes = Write(outgoing);
        var incoming = new MegaClilocPacket();

        var ok = incoming.TryParse(bytes);

        Assert.Multiple(
            () =>
            {
                Assert.That(ok, Is.True);
                Assert.That(incoming.IsClientRequest, Is.False);
                Assert.That(incoming.Serial, Is.EqualTo((Serial)0x40000011));
                Assert.That(incoming.Properties.Count, Is.EqualTo(1));
                Assert.That(incoming.Properties[0].ClilocId, Is.EqualTo(CommonClilocIds.ObjectName));
                Assert.That(incoming.Properties[0].Text, Is.EqualTo("Moongate"));
            }
        );
    }

    private static byte[] Write(ObjectPropertyList packet)
    {
        var writer = new SpanWriter(256, true);
        packet.Write(ref writer);
        var bytes = writer.ToArray();
        writer.Dispose();

        return bytes;
    }
}