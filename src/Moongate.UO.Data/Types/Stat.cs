using System.Text.Json.Serialization;
using Moongate.UO.Data.Json.Converters;

namespace Moongate.UO.Data.Types;

[JsonConverter(typeof(StatJsonConverter))]
public enum Stat
{
    Strength,
    Dexterity,
    Intelligence
}
