namespace Moongate.UO.Data.Maps;

[Flags]
public enum MapRules
{
    None = 0x0000,
    Internal = 0x0001,               // Internal map (used for dragging, commodity deeds, etc)
    FreeMovement = 0x0002,           // Anyone can move over anyone else without taking stamina loss
    BeneficialRestrictions = 0x0004, // Disallow performing beneficial actions on criminals/murderers
    HarmfulRestrictions = 0x0008,    // Disallow performing harmful actions on innocents
    TrammelRules = FreeMovement | BeneficialRestrictions | HarmfulRestrictions,
    FeluccaRules = None
}
