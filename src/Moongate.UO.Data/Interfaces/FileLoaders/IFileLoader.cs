namespace Moongate.UO.Data.Interfaces.FileLoaders;

/// <summary>
/// Defines an asynchronous loader for startup data files.
/// </summary>
public interface IFileLoader
{
    /// <summary>
    /// Loads the underlying data source.
    /// </summary>
    Task LoadAsync();
}
