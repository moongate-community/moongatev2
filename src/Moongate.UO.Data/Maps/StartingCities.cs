using Moongate.UO.Data.Packets.Data;

namespace Moongate.UO.Data.Maps;

public class StartingCities
{
    public static readonly CityInfo[] OldHavenStartingCities =
    [
        new("Haven", "The Bountiful Harvest Inn", 3677, 2625, 0, Map.Trammel),
        new("Britain", "Sweet Dreams Inn", 1075074, 1496, 1628, 10, Map.Trammel),
        new("Magincia", "The Great Horns Tavern", 1075077, 3734, 2222, 20, Map.Trammel)
    ];

    public static readonly CityInfo[] FeluccaStartingCities =
    [
        new("Yew", "The Empath Abbey", 1075072, 633, 858, 0, Map.Felucca),
        new("Minoc", "The Barnacle", 1075073, 2476, 413, 15, Map.Felucca),
        new("Britain", "Sweet Dreams Inn", 1075074, 1496, 1628, 10, Map.Felucca),
        new("Moonglow", "The Scholars Inn", 1075075, 4408, 1168, 0, Map.Felucca),
        new("Trinsic", "The Traveler's Inn", 1075076, 1845, 2745, 0, Map.Felucca),
        new("Magincia", "The Great Horns Tavern", 1075077, 3734, 2222, 20, Map.Felucca),
        new("Jhelom", "The Mercenary Inn", 1075078, 1374, 3826, 0, Map.Felucca),
        new("Skara Brae", "The Falconer's Inn", 1075079, 618, 2234, 0, Map.Felucca),
        new("Vesper", "The Ironwood Inn", 1075080, 2771, 976, 0, Map.Felucca)
    ];

    public static readonly CityInfo[] TrammelStartingCities =
    [
        new("Yew", "The Empath Abbey", 1075072, 633, 858, 0, Map.Trammel),
        new("Minoc", "The Barnacle", 1075073, 2476, 413, 15, Map.Trammel),
        new("Moonglow", "The Scholars Inn", 1075075, 4408, 1168, 0, Map.Trammel),
        new("Trinsic", "The Traveler's Inn", 1075076, 1845, 2745, 0, Map.Trammel),
        new("Jhelom", "The Mercenary Inn", 1075078, 1374, 3826, 0, Map.Trammel),
        new("Skara Brae", "The Falconer's Inn", 1075079, 618, 2234, 0, Map.Trammel),
        new("Vesper", "The Ironwood Inn", 1075080, 2771, 976, 0, Map.Trammel)
    ];

    public static readonly CityInfo[] NewHavenStartingCities =
    [
        new("New Haven", "The Bountiful Harvest Inn", 1150168, 3503, 2574, 14, Map.Trammel),
        new("Britain", "The Wayfarer's Inn", 1075074, 1602, 1591, 20, Map.Trammel)

        // Magincia removed because it burned down.
    ];

    public static readonly CityInfo[] StartingCitiesSA =
    [
        new("Royal City", "Royal City Inn", 1150169, 738, 3486, -19, Map.TerMur)
    ];

    public static CityInfo[] AvailableStartingCities => [..NewHavenStartingCities, ..TrammelStartingCities];
}
