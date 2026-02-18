using Moongate.UO.Data.Interfaces.FileLoaders;

namespace Moongate.Tests.Server.Support;

public sealed class FileLoaderServiceTestLoaderThrows : IFileLoader
{
    public Task LoadAsync()
        => throw new InvalidOperationException("boom");
}
