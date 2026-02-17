using Moongate.Abstractions.Interfaces.Services.Base;
using Moongate.UO.Data.Interfaces.FileLoaders;

namespace Moongate.Server.Interfaces.Services;

public interface IFileLoaderService : IMoongateService
{
    void AddFileLoader<T>() where T : IFileLoader;

    Task ExecuteLoadersAsync();
}
