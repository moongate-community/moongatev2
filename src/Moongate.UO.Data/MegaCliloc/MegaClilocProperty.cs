namespace Moongate.UO.Data.MegaCliloc;

/// <summary>
/// Represents a single cliloc property
/// </summary>
public class MegaClilocProperty
{
    /// <summary>
    /// Cliloc ID for this property
    /// </summary>
    public uint ClilocId { get; set; }

    /// <summary>
    /// Optional text to be inserted into the cliloc
    /// </summary>
    public string? Text { get; set; }

    public MegaClilocProperty(uint clilocId, string? text = null)
    {
        ClilocId = clilocId;
        Text = text;
    }

    public MegaClilocProperty()
    {
        ClilocId = 0;
        Text = null;
    }
}
