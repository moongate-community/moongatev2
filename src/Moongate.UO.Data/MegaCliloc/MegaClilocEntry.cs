using System.Globalization;
using Moongate.UO.Data.Ids;

namespace Moongate.UO.Data.MegaCliloc;

/// <summary>
/// Represents a cliloc entry for a specific object
/// </summary>
public class MegaClilocEntry
{
    /// <summary>
    /// Serial of the object/creature
    /// </summary>
    public Serial Serial { get; set; }

    /// <summary>
    /// List of properties/clilocs for this object
    /// </summary>
    public List<MegaClilocProperty> Properties { get; set; } = new();

    public MegaClilocEntry() { }

    public MegaClilocEntry(Serial serial, string name)
    {
        Serial = serial;

        Properties.Add(new(Serial.Value, name));
    }

    public void AddProperty(uint clilocId, params object[] text)
    {
        Properties.Add(new(clilocId, string.Join(" ", text)));
    }

    public void AddProperty(uint clilocId, string? text = null)
    {
        Properties.Add(new(clilocId, text));
    }

    public void AddProperty(uint clilocId, double value)
    {
        Properties.Add(new(clilocId, value.ToString(CultureInfo.InvariantCulture)));
    }

    public void AddProperty(uint clilocId, int value)
    {
        Properties.Add(new(clilocId, value.ToString(CultureInfo.InvariantCulture)));
    }
}
