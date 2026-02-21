using Moongate.Network.Packets.Incoming.Player;
using Moongate.UO.Data.Types;

namespace Moongate.Tests.Network.Packets;

public class GetPlayerStatusPacketTests
{
    [Test]
    public void TryParse_ShouldReadPatternTypeAndSerial()
    {
        var packet = new GetPlayerStatusPacket();
        var payload = new byte[]
        {
            0x34,
            0xED, 0xED, 0xED, 0xED,
            0x04,
            0x00, 0x00, 0x00, 0x02
        };

        var parsed = packet.TryParse(payload);

        Assert.Multiple(
            () =>
            {
                Assert.That(parsed, Is.True);
                Assert.That(packet.UnknownPattern, Is.EqualTo(0xEDEDEDEDu));
                Assert.That(packet.StatusType, Is.EqualTo(GetPlayerStatusType.BasicStatus));
                Assert.That(packet.MobileSerial, Is.EqualTo(0x00000002u));
            }
        );
    }

    [Test]
    public void TryParse_ShouldReturnFalse_WhenLengthIsInvalid()
    {
        var packet = new GetPlayerStatusPacket();
        var payload = new byte[] { 0x34, 0xED, 0xED, 0xED, 0xED, 0x04, 0x00, 0x00, 0x00 };

        var parsed = packet.TryParse(payload);

        Assert.That(parsed, Is.False);
    }
}
