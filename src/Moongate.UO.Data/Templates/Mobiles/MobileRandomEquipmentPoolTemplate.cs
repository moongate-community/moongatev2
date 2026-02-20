using Moongate.UO.Data.Types;

namespace Moongate.UO.Data.Templates.Mobiles;

/// <summary>
/// Random equipment pool used to roll gear for a mobile template.
/// </summary>
public class MobileRandomEquipmentPoolTemplate
{
    public string Name { get; set; }

    public ItemLayerType Layer { get; set; }

    public float SpawnChance { get; set; } = 1.0f;

    public List<MobileWeightedEquipmentItemTemplate> Items { get; set; } = [];
}
