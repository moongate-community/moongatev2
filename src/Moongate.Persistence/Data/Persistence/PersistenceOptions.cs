namespace Moongate.Persistence.Data.Persistence;

/// <summary>
/// Defines file paths used by snapshot and journal persistence.
/// </summary>
public sealed class PersistenceOptions
{
    public PersistenceOptions(string snapshotFilePath, string journalFilePath)
    {
        SnapshotFilePath = snapshotFilePath;
        JournalFilePath = journalFilePath;
    }

    public string SnapshotFilePath { get; }

    public string JournalFilePath { get; }
}
