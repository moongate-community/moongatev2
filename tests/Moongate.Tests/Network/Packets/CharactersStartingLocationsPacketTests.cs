using Moongate.Network.Packets.Outgoing.Login;
using Moongate.Network.Spans;
using Moongate.UO.Data.Packets.Data;
using Moongate.UO.Data.Persistence.Entities;

namespace Moongate.Tests.Network.Packets;

public class CharactersStartingLocationsPacketTests
{
    [Test]
    public void FillCharacters_ShouldPadToRequestedSize()
    {
        var packet = new CharactersStartingLocationsPacket();

        packet.FillCharacters([new CharacterEntry("alpha")], size: 7);

        Assert.That(packet.Characters.Count, Is.EqualTo(7));
        Assert.That(packet.Characters[0]?.Name, Is.EqualTo("alpha"));
        Assert.That(packet.Characters[1], Is.Null);
    }

    [Test]
    public void Write_WithOneCharacterAndNoCities_ShouldSerializeHeaderAndFlags()
    {
        var packet = new CharactersStartingLocationsPacket();
        packet.FillCharacters([new CharacterEntry("alpha")], size: 7);

        var writer = new SpanWriter(1024, true);
        packet.Write(ref writer);
        var data = writer.ToArray();
        writer.Dispose();

        var length = (data[1] << 8) | data[2];
        var flagsOffset = 3 + 1 + (7 * 60) + 1;
        var flags = (data[flagsOffset] << 24) |
                    (data[flagsOffset + 1] << 16) |
                    (data[flagsOffset + 2] << 8) |
                    data[flagsOffset + 3];
        var terminator = (short)((data[flagsOffset + 4] << 8) | data[flagsOffset + 5]);

        Assert.Multiple(
            () =>
            {
                Assert.That(data[0], Is.EqualTo(0xA9));
                Assert.That(length, Is.EqualTo(431));
                Assert.That(data[3], Is.EqualTo(7));
                Assert.That(data[4], Is.EqualTo((byte)'a'));
                Assert.That(data[5], Is.EqualTo((byte)'l'));
                Assert.That(data[6], Is.EqualTo((byte)'p'));
                Assert.That(data[7], Is.EqualTo((byte)'h'));
                Assert.That(data[8], Is.EqualTo((byte)'a'));
                Assert.That(data[424], Is.EqualTo(0));
                Assert.That((flags & 0x40) != 0, Is.True); // SixthCharacterSlot
                Assert.That((flags & 0x1000) != 0, Is.True); // SeventhCharacterSlot
                Assert.That(terminator, Is.EqualTo(-1));
            }
        );
    }

    [Test]
    public void FillCharacters_WithMobiles_ShouldMapNamesAndPad()
    {
        var packet = new CharactersStartingLocationsPacket();
        var mobiles = new List<UOMobileEntity>
        {
            new() { Name = "alpha" },
            new() { Name = null }
        };

        packet.FillCharacters(mobiles, size: 5);

        Assert.Multiple(
            () =>
            {
                Assert.That(packet.Characters.Count, Is.EqualTo(5));
                Assert.That(packet.Characters[0]?.Name, Is.EqualTo("alpha"));
                Assert.That(packet.Characters[1]?.Name, Is.EqualTo(string.Empty));
                Assert.That(packet.Characters[2], Is.Null);
            }
        );
    }
}
