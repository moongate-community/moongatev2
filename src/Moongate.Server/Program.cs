using ConsoleAppFramework;
using Moongate.Core.Utils;

await ConsoleApp.RunAsync(
    args,
    (bool showHeader = true) =>
    {
        if (showHeader)
        {
            ShowHeader();
        }
    }
);

return;

static void ShowHeader()
{
    var header = ResourceUtils.GetEmbeddedResourceString(typeof(Program).Assembly, "Resources/header.txt");

    Console.WriteLine(header);
    Console.WriteLine();
    Console.WriteLine("Platform: " + PlatformUtils.GetCurrentPlatform());
}
