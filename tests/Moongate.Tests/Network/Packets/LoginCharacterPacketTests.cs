using System.Buffers.Binary;
using Moongate.Network.Packets.Incoming.Login;
using Moongate.Network.Spans;
using Moongate.UO.Data.Types;

namespace Moongate.Tests.Network.Packets;

public class LoginCharacterPacketTests
{
    [Test]
    public void TryParse_WithValidPayload_ShouldPopulateFields()
    {
        var payload = BuildPayload();
        var packet = new LoginCharacterPacket();

        var parsed = packet.TryParse(payload);

        Assert.Multiple(
            () =>
            {
                Assert.That(parsed, Is.True);
                Assert.That(packet.Pattern1, Is.EqualTo(unchecked((int)0xEDEDEDED)));
                Assert.That(packet.CharacterName, Is.EqualTo("Avatar"));
                Assert.That(packet.Unknown0, Is.EqualTo((ushort)0x1234));
                Assert.That(packet.ClientFlags, Is.EqualTo(ClientFlags.Trammel | ClientFlags.Malas));
                Assert.That(packet.Unknown1, Is.EqualTo(0x01020304));
                Assert.That(packet.LoginCount, Is.EqualTo(7));
                Assert.That(
                    packet.Unknown2,
                    Is.EqualTo(new byte[16] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 })
                );
                Assert.That(packet.Slot, Is.EqualTo(2));
                Assert.That(packet.ClientIp, Is.EqualTo(0x7F000001u));
            }
        );
    }

    [Test]
    public void Write_ShouldSerializeExpectedPacketShape()
    {
        var packet = new LoginCharacterPacket
        {
            Pattern1 = unchecked((int)0xEDEDEDED),
            CharacterName = "Avatar",
            Unknown0 = 0x1234,
            ClientFlags = ClientFlags.Trammel | ClientFlags.Malas,
            Unknown1 = 0x01020304,
            LoginCount = 7,
            Unknown2 = new byte[16] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
            Slot = 2,
            ClientIp = 0x7F000001u
        };

        var data = Write(packet);

        Assert.Multiple(
            () =>
            {
                Assert.That(data.Length, Is.EqualTo(73));
                Assert.That(data[0], Is.EqualTo(0x5D));
                Assert.That(BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(1, 4)), Is.EqualTo(unchecked((int)0xEDEDEDED)));
                Assert.That(BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(41, 4)), Is.EqualTo(0x01020304));
                Assert.That(BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(45, 4)), Is.EqualTo(7));
                Assert.That(BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(65, 4)), Is.EqualTo(2));
                Assert.That(BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(69, 4)), Is.EqualTo(0x7F000001u));
            }
        );
    }

    private static byte[] BuildPayload()
    {
        var writer = new SpanWriter(73, true);
        writer.Write((byte)0x5D);
        writer.Write(unchecked((int)0xEDEDEDED));
        writer.WriteAscii("Avatar", 30);
        writer.Write((ushort)0x1234);
        writer.Write((uint)(ClientFlags.Trammel | ClientFlags.Malas));
        writer.Write(0x01020304);
        writer.Write(7);
        writer.Write(new byte[16] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });
        writer.Write(2);
        writer.Write(0x7F000001u);
        var data = writer.ToArray();
        writer.Dispose();

        return data;
    }

    private static byte[] Write(LoginCharacterPacket packet)
    {
        var writer = new SpanWriter(96, true);
        packet.Write(ref writer);
        var data = writer.ToArray();
        writer.Dispose();

        return data;
    }
}
