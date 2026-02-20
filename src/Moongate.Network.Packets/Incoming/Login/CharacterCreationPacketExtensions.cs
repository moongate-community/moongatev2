using Moongate.UO.Data.Geometry;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;
using Moongate.UO.Data.Types;

namespace Moongate.Network.Packets.Incoming.Login;

/// <summary>
/// Provides mapping helpers from character creation packet payloads to persistence entities.
/// </summary>
public static class CharacterCreationPacketExtensions
{
    /// <summary>
    /// Creates a new <see cref="UOMobileEntity" /> populated from this character creation packet.
    /// </summary>
    public static UOMobileEntity ToEntity(
        this CharacterCreationPacket packet,
        Serial mobileId,
        Serial accountId
    )
    {
        ArgumentNullException.ThrowIfNull(packet);

        var now = DateTime.UtcNow;
        var location = packet.StartingCity?.Location ?? Point3D.Zero;
        var mapId = packet.StartingCity?.Map?.Index ?? 0;
        var mobile = new UOMobileEntity
        {
            Id = mobileId,
            AccountId = accountId,
            Name = packet.CharacterName,
            Location = location,
            MapId = mapId,
            Direction = DirectionType.South,
            IsPlayer = true,
            IsAlive = true,
            Gender = packet.Gender,
            RaceIndex = (byte)Math.Max(0, packet.RaceIndex),
            ProfessionId = packet.ProfessionId,
            SkinHue = packet.Skin.Hue,
            HairStyle = packet.Hair.Style,
            HairHue = packet.Hair.Hue,
            FacialHairStyle = packet.FacialHair.Style,
            FacialHairHue = packet.FacialHair.Hue,
            Strength = packet.Strength,
            Dexterity = packet.Dexterity,
            Intelligence = packet.Intelligence,
            Hits = packet.Strength,
            Mana = packet.Intelligence,
            Stamina = packet.Dexterity,
            IsWarMode = false,
            IsHidden = false,
            IsFrozen = false,
            IsPoisoned = false,
            IsBlessed = false,
            Notoriety = Notoriety.Innocent,
            CreatedUtc = now,
            LastLoginUtc = now
        };

        mobile.RecalculateMaxStats();

        return mobile;
    }
}
