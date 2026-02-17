using ConsoleAppFramework;
using Moongate.Core.Types;
using Moongate.Core.Utils;
using Moongate.Server.Bootstrap;

await ConsoleApp.RunAsync(
    args,
    async (
        bool showHeader = true,
        string rootDirectory = null,
        LogLevelType loglevel = LogLevelType.Debug,
        CancellationToken cancellationToken = default
    ) =>
    {
        var bootstrap = new MoongateBootstrap(
            new()
            {
                RootDirectory = rootDirectory,
                LogLevel = loglevel,
                LogPacketData = true
            }
        );

        if (showHeader)
        {
            ShowHeader();
        }

        await bootstrap.RunAsync(cancellationToken);

        Console.WriteLine("Bye bye!");
    }
);

static void ShowHeader()
{
    var header = ResourceUtils.GetEmbeddedResourceString(typeof(Program).Assembly, "Resources/header.txt");

    Console.WriteLine();
    Console.WriteLine(header);
    Console.WriteLine();
    Console.WriteLine("Platform: " + PlatformUtils.GetCurrentPlatform());
}
