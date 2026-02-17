namespace Moongate.UO.Data.Json;

/// <summary>
/// JSON representation of a profession
/// </summary>
public class JsonProfession
{
    public string Name { get; set; } = string.Empty;
    public string TrueName { get; set; } = string.Empty;
    public int NameId { get; set; }
    public int DescId { get; set; }
    public int Desc { get; set; }
    public bool TopLevel { get; set; }
    public int Gump { get; set; }
    public string Type { get; set; } = string.Empty;
    public JsonSkill[] Skills { get; set; } = [];
    public JsonStat[] Stats { get; set; } = [];
}

/// <summary>
/// JSON representation of a skill
/// </summary>
public class JsonSkill
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}

/// <summary>
/// JSON representation of a stat
/// </summary>
public class JsonStat
{
    public string Type { get; set; } = string.Empty;
    public int Value { get; set; }
}

/// <summary>
/// Root object for JSON deserialization
/// </summary>
public class JsonProfessionsRoot
{
    public JsonProfession[] Professions { get; set; } = [];
}
