namespace Moongate.Server.Data.Config;

/// <summary>
/// Configures persistence autosave behavior.
/// </summary>
public class MoongatePersistenceConfig
{
    /// <summary>
    /// Gets or sets snapshot autosave interval in seconds.
    /// </summary>
    public int SaveIntervalSeconds { get; set; } = 30;
}
