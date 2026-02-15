using Moongate.Abstractions.Interfaces.Services;

namespace Moongate.Abstractions.Services.Base;

public abstract class BaseMoongateService : IMoongateService
{
    public Task StartAsync()
    {
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        return Task.CompletedTask;
    }
}
