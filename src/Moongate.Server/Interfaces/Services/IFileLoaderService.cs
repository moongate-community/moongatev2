using Moongate.Abstractions.Interfaces.Services.Base;
using Moongate.UO.Data.Interfaces.FileLoaders;

namespace Moongate.Server.Interfaces.Services;

/// <summary>
/// Manages registration and execution of startup file loaders.
/// </summary>
public interface IFileLoaderService : IMoongateService
{
    /// <summary>
    /// Registers a file loader type if not already present in the execution pipeline.
    /// </summary>
    /// <typeparam name="T">The file loader type.</typeparam>
    void AddFileLoader<T>() where T : IFileLoader;

    /// <summary>
    /// Executes all registered file loaders in their registration order.
    /// </summary>
    Task ExecuteLoadersAsync();
}
