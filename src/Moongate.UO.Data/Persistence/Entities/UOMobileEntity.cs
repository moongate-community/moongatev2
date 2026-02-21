using Moongate.UO.Data.Bodies;
using Moongate.UO.Data.Geometry;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Interfaces.Entities;
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
    private readonly Dictionary<ItemLayerType, ItemReference> _equippedItemReferences = [];

    public Serial Id { get; set; }

    public Serial AccountId { get; set; }

    public string? Name { get; set; }

    public string? Title { get; set; }

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

    public Body? BaseBody { get; set; }

    public Body Body
    {
        get => GetBody();
        set => SetBody(value);
    }

    public int Strength { get; set; }

    public int Dexterity { get; set; }

    public int Intelligence { get; set; }

    public int Hits { get; set; }

    public int Mana { get; set; }

    public int Stamina { get; set; }

    public int MaxHits { get; set; }

    public int MaxMana { get; set; }

    public int MaxStamina { get; set; }

    public int Level { get; set; } = 1;

    public long Experience { get; set; }

    public int SkillPoints { get; set; }

    public int StatPoints { get; set; }

    public int FireResistance { get; set; }

    public int ColdResistance { get; set; }

    public int PoisonResistance { get; set; }

    public int EnergyResistance { get; set; }

    public int Luck { get; set; }

    /// <summary>
    /// Gets or sets the serial of the backpack item.
    /// </summary>
    public Serial BackpackId { get; set; }

    /// <summary>
    /// Gets equipped item references by layer.
    /// </summary>
    public Dictionary<ItemLayerType, Serial> EquippedItemIds { get; set; } = [];

    /// <summary>
    /// Gets runtime equipped-item snapshots keyed by equipment layer.
    /// This cache is not used for persistence.
    /// </summary>
    public IReadOnlyDictionary<ItemLayerType, ItemReference> EquippedItemReferences => _equippedItemReferences;

    public bool IsWarMode { get; set; }

    public bool IsHidden { get; set; }

    public bool IsFrozen { get; set; }

    public bool IsParalyzed { get; set; }

    public bool IsFlying { get; set; }

    public bool IgnoreMobiles { get; set; }

    public bool IsPoisoned { get; set; }

    public bool IsBlessed { get; set; }

    public bool IsInvulnerable { get; set; }

    public bool IsMounted { get; set; }

    public Notoriety Notoriety { get; set; } = Notoriety.Innocent;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public DateTime LastLoginUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Associates an equipped item with this mobile and updates item ownership metadata.
    /// </summary>
    public void AddEquippedItem(ItemLayerType layer, UOItemEntity item)
    {
        ArgumentNullException.ThrowIfNull(item);

        EquippedItemIds[layer] = item.Id;
        _equippedItemReferences[layer] = new(item.Id, item.ItemId, item.Hue);
        item.ParentContainerId = Serial.Zero;
        item.ContainerPosition = Point2D.Zero;
        item.EquippedMobileId = Id;
        item.EquippedLayer = layer;
    }

    /// <summary>
    /// Associates an equipped item id with this mobile without item metadata updates.
    /// </summary>
    public void AddEquippedItem(ItemLayerType layer, Serial itemId)
    {
        EquippedItemIds[layer] = itemId;
        _equippedItemReferences.Remove(layer);
    }

    /// <summary>
    /// Equips an item and updates both persisted references and runtime cache.
    /// </summary>
    /// <param name="layer">Target item layer.</param>
    /// <param name="item">Equipped item entity.</param>
    public void EquipItem(ItemLayerType layer, UOItemEntity item)
        => AddEquippedItem(layer, item);

    public virtual Body GetBody()
    {
        if (BaseBody is Body baseBody)
        {
            if (baseBody == 0x00)
            {
                var raceForAliveBody = Race;

                return raceForAliveBody is null ? 0x00 : (Body)raceForAliveBody.Body(this);
            }

            return baseBody;
        }

        var fallbackRace = Race ?? (Race.Races.Length > 0 ? Race.Races[0] : null);

        return fallbackRace is null ? 0x00 : (Body)fallbackRace.Body(this);
    }

    /// <summary>
    /// Gets runtime equipped-item reference for a layer, if present.
    /// </summary>
    /// <param name="layer">Equipment layer.</param>
    /// <returns>Runtime equipped item reference; otherwise <c>null</c>.</returns>
    public ItemReference? GetEquippedReference(ItemLayerType layer)
    {
        if (_equippedItemReferences.TryGetValue(layer, out var itemReference))
        {
            return itemReference;
        }

        return null;
    }

    /// <summary>
    /// Calculates protocol packet flags for this mobile.
    /// </summary>
    /// <param name="stygianAbyss">
    /// Whether to use Stygian Abyss semantics (bit 0x04 is flying instead of poisoned).
    /// </param>
    /// <returns>Packet flags byte for mobile update packets.</returns>
    public virtual byte GetPacketFlags(bool stygianAbyss)
    {
        byte flags = 0x00;

        if (IsParalyzed || IsFrozen)
        {
            flags |= 0x01;
        }

        if (Gender == GenderType.Female)
        {
            flags |= 0x02;
        }

        if (stygianAbyss)
        {
            if (IsFlying)
            {
                flags |= 0x04;
            }
        }
        else
        {
            if (IsPoisoned)
            {
                flags |= 0x04;
            }
        }

        if (IsBlessed)
        {
            flags |= 0x08;
        }

        if (IgnoreMobiles)
        {
            flags |= 0x10;
            flags |= 0x40;
        }

        if (IsHidden)
        {
            flags |= 0x80;
        }

        return flags;
    }

    /// <summary>
    /// Gets whether an item is equipped in the specified layer.
    /// </summary>
    /// <param name="layer">Equipment layer.</param>
    /// <returns><c>true</c> when equipped.</returns>
    public bool HasEquippedItem(ItemLayerType layer)
        => EquippedItemIds.ContainsKey(layer);

    /// <summary>
    /// Hydrates runtime equipped-item references from resolved item entities.
    /// </summary>
    /// <param name="equippedItems">Resolved equipped items for this mobile.</param>
    public void HydrateEquipmentRuntime(IEnumerable<UOItemEntity> equippedItems)
    {
        ArgumentNullException.ThrowIfNull(equippedItems);

        _equippedItemReferences.Clear();

        foreach (var item in equippedItems)
        {
            if (item.EquippedMobileId != Id)
            {
                continue;
            }

            var layer = item.EquippedLayer;

            if (layer is null)
            {
                continue;
            }

            _equippedItemReferences[layer.Value] = new(item.Id, item.ItemId, item.Hue);
        }
    }

    public void OverrideBody(Body body)
        => SetBody(body);

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

    public void SetBody(Body body)
        => BaseBody = body;

    public override string ToString()
        => $"Mobile(Id={Id}, IsPlayer={IsPlayer}, Location={Location})";

    /// <summary>
    /// Tries to get runtime equipped-item reference for a layer.
    /// </summary>
    /// <param name="layer">Equipment layer.</param>
    /// <param name="itemReference">Resolved runtime reference when found.</param>
    /// <returns><c>true</c> when runtime reference is available.</returns>
    public bool TryGetEquippedReference(ItemLayerType layer, out ItemReference itemReference)
        => _equippedItemReferences.TryGetValue(layer, out itemReference);

    /// <summary>
    /// Unequips an item layer and optionally clears metadata on a provided item instance.
    /// </summary>
    /// <param name="layer">Layer to unequip.</param>
    /// <param name="item">Optional equipped item instance to clear metadata on.</param>
    /// <returns><c>true</c> when a layer entry existed and was removed.</returns>
    public bool UnequipItem(ItemLayerType layer, UOItemEntity? item = null)
    {
        var removed = EquippedItemIds.Remove(layer);
        _equippedItemReferences.Remove(layer);

        if (removed && item is not null)
        {
            item.EquippedMobileId = Serial.Zero;
            item.EquippedLayer = null;
        }

        return removed;
    }
}
