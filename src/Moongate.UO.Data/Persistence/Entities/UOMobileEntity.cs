using Moongate.UO.Data.Geometry;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Interfaces.Entities;
using Moongate.UO.Data.Maps;
using Moongate.UO.Data.Professions;
using Moongate.UO.Data.Races.Base;
using Moongate.UO.Data.Types;
using UoMap = Moongate.UO.Data.Maps.Map;

namespace Moongate.UO.Data.Persistence.Entities;

/// <summary>
/// Minimal mobile entity implementation used by race and map systems.
/// </summary>
public class UOMobileEntity : IMobileEntity
{
    public Serial Id { get; set; }

    public Serial AccountId { get; set; }

    public string? Name { get; set; }

    public Point3D Location { get; set; }

    public int MapId { get; set; }

    public UoMap? Map
    {
        get => UoMap.GetMap(MapId);
        set => MapId = value?.Index ?? 0;
    }

    public DirectionType Direction { get; set; }

    public bool IsPlayer { get; set; }

    public bool IsAlive { get; set; } = true;

    public GenderType Gender { get; set; }

    public byte RaceIndex { get; set; }

    public Race? Race
    {
        get => RaceIndex < Race.Races.Length ? Race.Races[RaceIndex] : null;
        set => RaceIndex = value is null ? (byte)0 : (byte)value.RaceIndex;
    }

    public int ProfessionId { get; set; }

    public ProfessionInfo Profession
    {
        get
        {
            if (ProfessionInfo.Professions is { Length: > 0 } &&
                ProfessionInfo.GetProfession(ProfessionId, out var profession))
            {
                return profession;
            }

            return new() { ID = ProfessionId };
        }
        set => ProfessionId = value?.ID ?? 0;
    }

    public short SkinHue { get; set; }

    public short HairStyle { get; set; }

    public short HairHue { get; set; }

    public short FacialHairStyle { get; set; }

    public short FacialHairHue { get; set; }

    public int Strength { get; set; }

    public int Dexterity { get; set; }

    public int Intelligence { get; set; }

    public int Hits { get; set; }

    public int Mana { get; set; }

    public int Stamina { get; set; }

    public int MaxHits { get; set; }

    public int MaxMana { get; set; }

    public int MaxStamina { get; set; }

    public bool IsWarMode { get; set; }

    public bool IsHidden { get; set; }

    public bool IsFrozen { get; set; }

    public bool IsPoisoned { get; set; }

    public bool IsBlessed { get; set; }

    public Notoriety Notoriety { get; set; } = Notoriety.Innocent;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public DateTime LastLoginUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Recomputes max stat caps from base stats and clamps current values.
    /// </summary>
    public void RecalculateMaxStats()
    {
        MaxHits = Math.Max(1, Strength);
        MaxMana = Math.Max(1, Intelligence);
        MaxStamina = Math.Max(1, Dexterity);

        Hits = Math.Min(Hits, MaxHits);
        Mana = Math.Min(Mana, MaxMana);
        Stamina = Math.Min(Stamina, MaxStamina);
    }

    public override string ToString()
        => $"Mobile(Id={Id}, IsPlayer={IsPlayer}, Location={Location})";
}
