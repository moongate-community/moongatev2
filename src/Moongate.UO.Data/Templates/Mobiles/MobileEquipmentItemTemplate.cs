using Moongate.UO.Data.Types;

namespace Moongate.UO.Data.Templates.Mobiles;

/// <summary>
/// Fixed equipment entry for a mobile template.
/// </summary>
public class MobileEquipmentItemTemplate
{
    public string ItemTemplateId { get; set; }

    public ItemLayerType Layer { get; set; }
}
