using Moongate.UO.Data.Races.Base;

namespace Moongate.UO.Data.Races;

public class RaceDefinitions
{
    public static void RegisterRace(Race race)
    {
        Race.Races[race.RaceIndex] = race;
        Race.AllRaces.Add(race);
    }
}
