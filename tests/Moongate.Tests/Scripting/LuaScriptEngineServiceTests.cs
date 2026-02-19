using DryIoc;
using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Scripting.Data.Scripts;
using Moongate.Scripting.Modules;
using Moongate.Scripting.Services;
using Moongate.Tests.TestSupport;

namespace Moongate.Tests.Scripting;

public class LuaScriptEngineServiceTests
{
    [Test]
    public void AddCallback_AndExecuteCallback_ShouldInvokeCallback()
    {
        using var temp = new TempDirectory();
        var service = CreateService(temp.Path);
        object[]? captured = null;

        service.AddCallback("onTest", args => captured = args);
        service.ExecuteCallback("onTest", 1, "two");

        Assert.That(captured, Is.Not.Null);
        Assert.That(captured!.Length, Is.EqualTo(2));
        Assert.That(captured[0], Is.EqualTo(1));
        Assert.That(captured[1], Is.EqualTo("two"));
    }

    [Test]
    public void AddConstant_ShouldExposeNormalizedGlobal()
    {
        using var temp = new TempDirectory();
        var service = CreateService(temp.Path);

        service.AddConstant("myValue", 42);
        var result = service.ExecuteFunction("MY_VALUE");

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Success, Is.True);
                Assert.That(result.Data, Is.EqualTo(42d));
            }
        );
    }

    [Test]
    public void AddManualModuleFunction_ShouldBeCallableFromLua()
    {
        using var temp = new TempDirectory();
        var service = CreateService(temp.Path);

        service.AddManualModuleFunction<int, int>("math", "double", static value => value * 2);
        var result = service.ExecuteFunction("math.double(21)");

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Success, Is.True);
                Assert.That(result.Data, Is.EqualTo(42d));
            }
        );
    }

    [Test]
    public void ExecuteFunction_WhenLuaError_ShouldReturnErrorAndRaiseEvent()
    {
        using var temp = new TempDirectory();
        var service = CreateService(temp.Path);
        ScriptErrorInfo? capturedError = null;
        service.OnScriptError += (_, info) => capturedError = info;

        var result = service.ExecuteFunction("unknown_function()");

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Success, Is.False);
                Assert.That(result.Message, Is.Not.Empty);
                Assert.That(capturedError, Is.Not.Null);
            }
        );
    }

    [Test]
    public void ExecuteScriptFile_WhenFileMissing_ShouldThrowFileNotFoundException()
    {
        using var temp = new TempDirectory();
        var service = CreateService(temp.Path);
        var file = Path.Combine(temp.Path, "scripts", "missing.lua");

        Assert.That(() => service.ExecuteScriptFile(file), Throws.TypeOf<FileNotFoundException>());
    }

    [Test]
    public async Task StartAsync_WithLogModule_ShouldKeepLogAsTable()
    {
        using var temp = new TempDirectory();
        var dirs = new DirectoriesConfig(temp.Path, Enum.GetNames<DirectoryType>());
        var scriptsDir = dirs[DirectoryType.Scripts];
        var luarcDir = Path.Combine(temp.Path, ".luarc");
        Directory.CreateDirectory(scriptsDir);
        Directory.CreateDirectory(luarcDir);

        var service = new LuaScriptEngineService(
            dirs,
            [new(typeof(LogModule))],
            new Container(),
            new(luarcDir, scriptsDir, "0.1.0"),
            []
        );

        await service.StartAsync();

        var typeResult = service.ExecuteFunction("type(log)");
        var callResult = service.ExecuteFunction("log.info('hello')");

        Assert.Multiple(
            () =>
            {
                Assert.That(typeResult.Success, Is.True);
                Assert.That(typeResult.Data, Is.EqualTo("table"));
                Assert.That(callResult.Success, Is.True);
            }
        );
    }

    [Test]
    public void ToScriptEngineFunctionName_ShouldConvertToSnakeCase()
    {
        using var temp = new TempDirectory();
        var service = CreateService(temp.Path);

        var name = service.ToScriptEngineFunctionName("HelloWorldMethod");

        Assert.That(name, Is.EqualTo("hello_world_method"));
    }

    private static LuaScriptEngineService CreateService(string rootPath)
    {
        var dirs = new DirectoriesConfig(rootPath, Enum.GetNames<DirectoryType>());
        var scriptsDir = dirs[DirectoryType.Scripts];
        var luarcDir = Path.Combine(rootPath, ".luarc");
        Directory.CreateDirectory(scriptsDir);
        Directory.CreateDirectory(luarcDir);

        return new(
            dirs,
            [],
            new Container(),
            new(luarcDir, scriptsDir, "0.1.0"),
            []
        );
    }
}
