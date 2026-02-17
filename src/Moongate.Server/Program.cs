using ConsoleAppFramework;
using Moongate.Core.Utils;
using Moongate.Server.Bootstrap;
using Serilog;

await ConsoleApp.RunAsync(
    args,
    async (bool showHeader = true, CancellationToken cancellationToken = default) =>
    {
        Log.Logger = new LoggerConfiguration()
                     .MinimumLevel
                     .Verbose()
                     .WriteTo
                     .Console()
                     .CreateLogger();

        var bootstrap = new MoongateBootstrap();

        if (showHeader)
        {
            ShowHeader();
        }

        await bootstrap.RunAsync(cancellationToken);
    }
);

static void ShowHeader()
{
    var header = ResourceUtils.GetEmbeddedResourceString(typeof(Program).Assembly, "Resources/header.txt");

    Console.WriteLine(header);
    Console.WriteLine();
    Console.WriteLine("Platform: " + PlatformUtils.GetCurrentPlatform());
}
