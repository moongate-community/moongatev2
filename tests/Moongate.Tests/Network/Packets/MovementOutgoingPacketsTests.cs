using System.Buffers.Binary;
using Moongate.Network.Packets.Outgoing.Movement;
using Moongate.Network.Spans;
using Moongate.UO.Data.Types;

namespace Moongate.Tests.Network.Packets;

public class MovementOutgoingPacketsTests
{
    [Test]
    public void MoveConfirmPacket_Write_ShouldSerializeSequenceAndNotoriety()
    {
        var packet = new MoveConfirmPacket(sequence: 7, notoriety: 3);

        var data = Write(packet);

        Assert.That(data, Is.EqualTo(new byte[] { 0x22, 0x07, 0x03 }));
    }

    [Test]
    public void MoveDenyPacket_Write_ShouldSerializePositionDirectionAndZ()
    {
        var packet = new MoveDenyPacket(sequence: 9, x: 1200, y: 1300, direction: DirectionType.South, z: -5);

        var data = Write(packet);

        Assert.Multiple(
            () =>
            {
                Assert.That(data.Length, Is.EqualTo(8));
                Assert.That(data[0], Is.EqualTo(0x21));
                Assert.That(data[1], Is.EqualTo(0x09));
                Assert.That(BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(2, 2)), Is.EqualTo((ushort)1200));
                Assert.That(BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(4, 2)), Is.EqualTo((ushort)1300));
                Assert.That(data[6], Is.EqualTo((byte)DirectionType.South));
                Assert.That(unchecked((sbyte)data[7]), Is.EqualTo(-5));
            }
        );
    }

    [Test]
    public void MovePlayerPacket_Write_ShouldSerializeDirection()
    {
        var packet = new MovePlayerPacket(DirectionType.West | DirectionType.Running);

        var data = Write(packet);

        Assert.That(data, Is.EqualTo(new byte[] { 0x97, 0x86 }));
    }

    [Test]
    public void MovementSpeedControlPacket_Write_ShouldSerializeGeneralInformationSubcommand()
    {
        var packet = new MovementSpeedControlPacket(MovementSpeedControlType.Mount);

        var data = Write(packet);

        Assert.That(data, Is.EqualTo(new byte[] { 0xBF, 0x00, 0x06, 0x00, 0x26, 0x01 }));
    }

    [Test]
    public void TimeSyncResponsePacket_Write_ShouldSerializeThreeTickValues()
    {
        var packet = new TimeSyncResponsePacket(123, 456, 789);

        var data = Write(packet);

        Assert.Multiple(
            () =>
            {
                Assert.That(data.Length, Is.EqualTo(13));
                Assert.That(data[0], Is.EqualTo(0xF2));
                Assert.That(BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(1, 4)), Is.EqualTo(123));
                Assert.That(BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(5, 4)), Is.EqualTo(456));
                Assert.That(BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(9, 4)), Is.EqualTo(789));
            }
        );
    }

    private static byte[] Write(Moongate.Network.Packets.Interfaces.IGameNetworkPacket packet)
    {
        var writer = new SpanWriter(64, true);
        packet.Write(ref writer);
        var data = writer.ToArray();
        writer.Dispose();

        return data;
    }
}
