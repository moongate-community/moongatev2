namespace Moongate.UO.Data.Templates.Mobiles;

/// <summary>
/// Weighted item option used by random equipment pools.
/// </summary>
public class MobileWeightedEquipmentItemTemplate
{
    public string ItemTemplateId { get; set; }

    public int Weight { get; set; } = 1;
}
