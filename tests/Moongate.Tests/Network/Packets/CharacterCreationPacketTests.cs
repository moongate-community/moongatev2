using Moongate.Network.Packets.Incoming.Login;
using Moongate.Network.Spans;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Types;

namespace Moongate.Tests.Network.Packets;

public class CharacterCreationPacketTests
{
    [Test]
    public void TryParse_WithValidPayload_ShouldPopulateFields()
    {
        var payload = BuildPayload();
        var packet = new CharacterCreationPacket();

        var parsed = packet.TryParse(payload);

        Assert.Multiple(
            () =>
            {
                Assert.That(parsed, Is.True);
                Assert.That(packet.CharacterName, Is.EqualTo("TestCharacter"));
                Assert.That(packet.ClientFlags.HasFlag(ClientFlags.Trammel), Is.True);
                Assert.That(packet.ProfessionId, Is.EqualTo(2));
                Assert.That(packet.Gender, Is.EqualTo(GenderType.Female));
                Assert.That(packet.RaceIndex, Is.EqualTo(1));
                Assert.That(packet.Strength, Is.EqualTo(60));
                Assert.That(packet.Dexterity, Is.EqualTo(50));
                Assert.That(packet.Intelligence, Is.EqualTo(40));
                Assert.That(packet.Skills.Count, Is.EqualTo(4));
                Assert.That(packet.Skills[0].Skill, Is.EqualTo(UOSkillName.Magery));
                Assert.That(packet.Skills[0].Value, Is.EqualTo(50));
                Assert.That(packet.StartingCityIndex, Is.EqualTo(3));
                Assert.That(packet.Slot, Is.EqualTo(1));
                Assert.That(packet.Skin.Style, Is.EqualTo(0));
                Assert.That(packet.Skin.Hue, Is.EqualTo(0x0455));
                Assert.That(packet.Hair.Style, Is.EqualTo(0x0203));
                Assert.That(packet.Hair.Hue, Is.EqualTo(0x0304));
                Assert.That(packet.FacialHair.Style, Is.EqualTo(0x0506));
                Assert.That(packet.FacialHair.Hue, Is.EqualTo(0x0708));
                Assert.That(packet.Shirt.Style, Is.EqualTo(0));
                Assert.That(packet.Shirt.Hue, Is.EqualTo(0x0888));
                Assert.That(packet.Pants.Style, Is.EqualTo(0));
                Assert.That(packet.Pants.Hue, Is.EqualTo(0x0999));
            }
        );
    }

    [Test]
    public void TryParse_WithInvalidGenderRaceByte_ShouldReturnFalse()
    {
        var payload = BuildPayload(genderAndRace: 0xFF);
        var packet = new CharacterCreationPacket();

        var parsed = packet.TryParse(payload);

        Assert.That(parsed, Is.False);
    }

    [Test]
    public void ToEntity_ShouldMapPacketIntoMobileEntity()
    {
        var payload = BuildPayload();
        var packet = new CharacterCreationPacket();
        _ = packet.TryParse(payload);

        var mobile = packet.ToEntity((Serial)0x1001, (Serial)0x2001);

        Assert.Multiple(
            () =>
            {
                Assert.That(mobile.Id, Is.EqualTo((Serial)0x1001));
                Assert.That(mobile.AccountId, Is.EqualTo((Serial)0x2001));
                Assert.That(mobile.Name, Is.EqualTo("TestCharacter"));
                Assert.That(mobile.IsPlayer, Is.True);
                Assert.That(mobile.IsAlive, Is.True);
                Assert.That(mobile.Gender, Is.EqualTo(GenderType.Female));
                Assert.That(mobile.RaceIndex, Is.EqualTo(1));
                Assert.That(mobile.ProfessionId, Is.EqualTo(2));
                Assert.That(mobile.SkinHue, Is.EqualTo(0x0455));
                Assert.That(mobile.HairStyle, Is.EqualTo(0x0203));
                Assert.That(mobile.HairHue, Is.EqualTo(0x0304));
                Assert.That(mobile.FacialHairStyle, Is.EqualTo(0x0506));
                Assert.That(mobile.FacialHairHue, Is.EqualTo(0x0708));
                Assert.That(mobile.Strength, Is.EqualTo(60));
                Assert.That(mobile.Dexterity, Is.EqualTo(50));
                Assert.That(mobile.Intelligence, Is.EqualTo(40));
                Assert.That(mobile.Hits, Is.EqualTo(60));
                Assert.That(mobile.MaxHits, Is.EqualTo(60));
                Assert.That(mobile.Stamina, Is.EqualTo(50));
                Assert.That(mobile.MaxStamina, Is.EqualTo(50));
                Assert.That(mobile.Mana, Is.EqualTo(40));
                Assert.That(mobile.MaxMana, Is.EqualTo(40));
            }
        );
    }

    [Test]
    public void UOMobileEntity_RuntimeProperties_ShouldMapIdBackedValues()
    {
        var payload = BuildPayload();
        var packet = new CharacterCreationPacket();
        _ = packet.TryParse(payload);

        var mobile = packet.ToEntity((Serial)0x1002, (Serial)0x2002);
        mobile.Profession = new() { ID = 9 };

        Assert.Multiple(
            () =>
            {
                Assert.That(mobile.ProfessionId, Is.EqualTo(9));
                Assert.That(mobile.Profession.ID, Is.EqualTo(9));
            }
        );
    }

    private static byte[] BuildPayload(byte genderAndRace = 5)
    {
        var writer = new SpanWriter(106, true);

        writer.Write((byte)0xF8);
        writer.Write(unchecked((int)0xEDEDEDED));
        writer.Write(unchecked((int)0xFFFFFFFF));
        writer.Write((byte)0x00);
        writer.WriteAscii("TestCharacter", 30);
        writer.Write((ushort)0);
        writer.Write((uint)ClientFlags.Trammel);
        writer.Write(0);
        writer.Write(0);
        writer.Write((byte)2);
        writer.Clear(15);
        writer.Write(genderAndRace);
        writer.Write((byte)60);
        writer.Write((byte)50);
        writer.Write((byte)40);
        writer.Write((byte)UOSkillName.Magery);
        writer.Write((byte)50);
        writer.Write((byte)UOSkillName.Meditation);
        writer.Write((byte)50);
        writer.Write((byte)UOSkillName.EvalInt);
        writer.Write((byte)50);
        writer.Write((byte)UOSkillName.Wrestling);
        writer.Write((byte)50);
        writer.Write((short)0x0455);
        writer.Write((short)0x0203);
        writer.Write((short)0x0304);
        writer.Write((short)0x0506);
        writer.Write((short)0x0708);
        writer.Write((short)3);
        writer.Write((ushort)0);
        writer.Write((short)1);
        writer.Write(0);
        writer.Write((short)0x0888);
        writer.Write((short)0x0999);

        var result = writer.ToArray();
        writer.Dispose();
        return result;
    }
}
