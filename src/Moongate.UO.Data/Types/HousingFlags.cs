namespace Moongate.UO.Data.Types;

[Flags]
public enum HousingFlags
{
    None = 0x0,
    AOS = 0x10,
    SE = 0x40,
    ML = 0x80,
    Crystal = 0x200,
    SA = 0x10000,
    HS = 0x20000,
    Gothic = 0x40000,
    Rustic = 0x80000,
    Jungle = 0x100000,
    Shadowguard = 0x200000,
    TOL = 0x400000,
    EJ = 0x800000,

    HousingAOS = AOS,
    HousingSE = HousingAOS | SE,
    HousingML = HousingSE | ML | Crystal,
    HousingSA = HousingML | SA | Gothic | Rustic,
    HousingHS = HousingSA | HS,
    HousingTOL = HousingHS | TOL | Jungle | Shadowguard,
    HousingEJ = HousingTOL | EJ
}
