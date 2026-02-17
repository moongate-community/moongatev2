namespace Moongate.Scripting.Data.Scripts;

/// <summary>
/// Metrics about script execution performance.
/// </summary>
public class ScriptExecutionMetrics
{
    /// <summary>
    /// Gets or sets the execution time in milliseconds.
    /// </summary>
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the memory used in bytes.
    /// </summary>
    public long MemoryUsedBytes { get; set; }

    /// <summary>
    /// Gets or sets the number of statements executed.
    /// </summary>
    public int StatementsExecuted { get; set; }

    /// <summary>
    /// Gets or sets the number of cache hits.
    /// </summary>
    public int CacheHits { get; set; }

    /// <summary>
    /// Gets or sets the number of cache misses.
    /// </summary>
    public int CacheMisses { get; set; }

    /// <summary>
    /// Gets or sets the total number of scripts cached.
    /// </summary>
    public int TotalScriptsCached { get; set; }
}
