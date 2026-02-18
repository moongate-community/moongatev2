using ConsoleAppFramework;
using Moongate.Core.Json;
using Moongate.Core.Types;
using Moongate.Core.Utils;
using Moongate.Scripting.Context;
using Moongate.Server.Bootstrap;
using Moongate.Server.Json;
using Moongate.UO.Data.Json.Converters;
using Moongate.UO.Data.Json.Context;

await ConsoleApp.RunAsync(
    args,
    async (
        bool showHeader = true,
        string rootDirectory = null,
        string uoDirectory = null,
        LogLevelType loglevel = LogLevelType.Debug,
        CancellationToken cancellationToken = default
    ) =>
    {

        if (showHeader)
        {
            ShowHeader();
        }


        JsonUtils.AddJsonConverter(new ClientVersionConverter());
        JsonUtils.AddJsonConverter(new MapConverter());
        JsonUtils.AddJsonConverter(new Point2DConverter());
        JsonUtils.AddJsonConverter(new Point3DConverter());
        JsonUtils.AddJsonConverter(new ProfessionInfoConverter());
        JsonUtils.AddJsonConverter(new RaceConverter());
        JsonUtils.AddJsonConverter(new SerialConverter());

        JsonUtils.RegisterJsonContext(MoongateUOJsonSerializationContext.Default);
        JsonUtils.RegisterJsonContext(MoongateServerJsonContext.Default);
        JsonUtils.RegisterJsonContext(MoongateLuaScriptJsonContext.Default);

        var bootstrap = new MoongateBootstrap(
            new()
            {
                RootDirectory = rootDirectory,
                LogLevel = loglevel,
                LogPacketData = true,
                UODirectory = uoDirectory
            }
        );


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
