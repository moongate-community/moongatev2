namespace Moongate.Abstractions.Interfaces.Services;

public interface IMoongateService
{
    Task StartAsync();

    Task StopAsync();
}
