using System.Buffers.Binary;
using Moongate.Network.Packets.Outgoing.Login;
using Moongate.Network.Spans;
using Moongate.UO.Data.Types;

namespace Moongate.Tests.Network.Packets;

public class SupportFeaturesPacketTests
{
    [Test]
    public void Write_ShouldSerializeOpcodeAndFeatureFlags()
    {
        const FeatureFlags flags = FeatureFlags.LiveAccount | FeatureFlags.SeventhCharacterSlot;
        var packet = new SupportFeaturesPacket(flags);

        var writer = new SpanWriter(16, true);
        packet.Write(ref writer);
        var data = writer.ToArray();
        writer.Dispose();

        Assert.Multiple(
            () =>
            {
                Assert.That(data.Length, Is.EqualTo(5));
                Assert.That(data[0], Is.EqualTo(0xB9));
                Assert.That(
                    BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(1, 4)),
                    Is.EqualTo((uint)flags)
                );
            }
        );
    }
}
