using Moongate.Network.Packets.Incoming.Movement;
using Moongate.UO.Data.Types;

namespace Moongate.Tests.Network.Packets;

public class MoveRequestPacketTests
{
    [Test]
    public void TryParse_ShouldReadDirectionSequenceAndFastWalkKey()
    {
        var packet = new MoveRequestPacket();
        var payload = new byte[] { 0x02, (byte)(DirectionType.East | DirectionType.Running), 0x7F, 0x12, 0x34, 0x56, 0x78 };

        var parsed = packet.TryParse(payload);

        Assert.Multiple(
            () =>
            {
                Assert.That(parsed, Is.True);
                Assert.That(packet.Direction, Is.EqualTo(DirectionType.East | DirectionType.Running));
                Assert.That(packet.WalkDirection, Is.EqualTo(DirectionType.East));
                Assert.That(packet.IsRunning, Is.True);
                Assert.That(packet.Sequence, Is.EqualTo(0x7F));
                Assert.That(packet.FastWalkKey, Is.EqualTo(0x12345678u));
            }
        );
    }

    [Test]
    public void TryParse_ShouldReturnFalse_WhenLengthIsInvalid()
    {
        var packet = new MoveRequestPacket();
        var payload = new byte[] { 0x02, 0x01, 0x02, 0x03, 0x04, 0x05 };

        var parsed = packet.TryParse(payload);

        Assert.That(parsed, Is.False);
    }
}
